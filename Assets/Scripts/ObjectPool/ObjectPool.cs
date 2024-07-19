using System;
using System.Collections.Generic;
using UnityEngine;

// A component for tracking the lifecycle of pooled objects
public class PoolableObjectTracker : MonoBehaviour
{
    private PoolableObject trackedObject;
    public event Action<PoolableObject> OnDisabled;

    private void Awake()
    {
        trackedObject = GetComponent<PoolableObject>();
    }

    private void OnDisable()
    {
        OnDisabled?.Invoke(trackedObject);
    }
}

// ObjectPool specifically for PoolableObject
public class ObjectPool
{
    private PoolableObject prefab;
    private Stack<PoolableObject> spawnedObjects = new Stack<PoolableObject>(); // Stack for fast pushing and popping

    public static ObjectPool CreateInstance(PoolableObject prefab, int initialSize)
    {
        if (prefab == null)
        {
            throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null");
        }

        ObjectPool pool = new ObjectPool
        {
            prefab = prefab
        };
        pool.AddNewObjectsToPool(initialSize);
        return pool;
    }

    public PoolableObject GetObject()
    {
        if (spawnedObjects.Count == 0)
        {
            AddNewObjectsToPool(1);
        }

        PoolableObject pooledObject = spawnedObjects.Pop();
        pooledObject.gameObject.SetActive(true);
        return pooledObject;
    }

    private void AddNewObjectsToPool(int objectCount)
    {
        for (int i = 0; i < objectCount; ++i)
        {
            PoolableObject pooledObject = UnityEngine.Object.Instantiate(prefab);
            pooledObject.gameObject.SetActive(false);

            // Assign the parent pool to the PoolableObject
            pooledObject.Parent = this;

            // Start tracking the object's lifecycle
            PoolableObjectTracker tracker = pooledObject.gameObject.AddComponent<PoolableObjectTracker>();
            tracker.OnDisabled += ReturnObjectToPool;
            spawnedObjects.Push(pooledObject);
        }
    }

    public void ReturnObjectToPool(PoolableObject pooledObject)
    {
        pooledObject.gameObject.SetActive(false);
        spawnedObjects.Push(pooledObject);
    }

    private void OnDestroy()
    {
        // Detach from all trackers to avoid errors on scene deinitialization
        foreach (var pooledObject in spawnedObjects)
        {
            PoolableObjectTracker tracker = pooledObject.GetComponent<PoolableObjectTracker>();
            if (tracker != null)
            {
                tracker.OnDisabled -= ReturnObjectToPool;
            }
        }
    }
}