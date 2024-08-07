﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using GenericObjectPool;

public class RangedAttackRadius : AttackRadius
{
    public NavMeshAgent Agent;
    public Bullet BulletPrefab;
    public Vector3 BulletSpawnOffset = new Vector3(0, 1, 0);
    public LayerMask Mask;
    private GenericObjectPool<Bullet> BulletPool;
    [SerializeField]
    private float SpherecastRadius = 0.1f;
    private RaycastHit Hit;
    private IDamageable targetDamageable;
    private Bullet bullet;

    protected override void Awake()
    {
        base.Awake();

        BulletPool = GenericObjectPool<Bullet>.CreateInstance(BulletPrefab, Mathf.CeilToInt((1 / AttackDelay) * BulletPrefab.AutoDestroyTime));
    }

    protected override IEnumerator Attack()
    {
        WaitForSeconds Wait = new WaitForSeconds(AttackDelay);

        yield return Wait;

        while (Damageables.Count > 0)
        {
            for (int i = 0; i < Damageables.Count; i++)
            {
                if (HasLineOfSightTo(Damageables[i].GetTransform()))
                {
                    targetDamageable = Damageables[i];
                    OnAttack?.Invoke(Damageables[i]);
                    Agent.enabled = false;
                    break;
                }
            }

            if (targetDamageable != null)
            {
                Bullet poolableBullet = BulletPool.GetObject();
                if (poolableBullet != null)
                {
                    bullet = poolableBullet.GetComponent<Bullet>();

                    bullet.Damage = Damage;
                    bullet.transform.position = transform.position + BulletSpawnOffset;
                    bullet.transform.rotation = Agent.transform.rotation;
                    bullet.Rigidbody.AddForce(Agent.transform.forward * BulletPrefab.MoveSpeed, ForceMode.VelocityChange);
                }
            }
            else
            {
                Agent.enabled = true; // no target in line of sight, keep trying to get closer
            }

            yield return Wait;

            if (targetDamageable == null || !HasLineOfSightTo(targetDamageable.GetTransform()))
            {
                Agent.enabled = true;
            }

            Damageables.RemoveAll(DisabledDamageables);
        }

        Agent.enabled = true;
        AttackCoroutine = null;
    }

    private bool HasLineOfSightTo(Transform Target)
    {
        if (Physics.SphereCast(transform.position + BulletSpawnOffset, SpherecastRadius, ((Target.position + BulletSpawnOffset) - (transform.position + BulletSpawnOffset)).normalized, out Hit, Collider.radius, Mask))
        {
            IDamageable damageable;
            if (Hit.collider.TryGetComponent<IDamageable>(out damageable))
            {
                return damageable.GetTransform() == Target;
            }
        }

        return false;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if (AttackCoroutine == null)
        {
            Agent.enabled = true;
        }
    }
}
