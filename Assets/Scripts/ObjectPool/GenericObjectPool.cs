using System;
using System.Collections.Generic;
using UnityEngine;

namespace GenericObjectPool
{
    public class GenericObjectPool<T> where T : Component
    {
        private T prefab;
        private Stack<T> spawnedObjects = new Stack<T>();

        public static GenericObjectPool<T> CreateInstance(T prefab, int initialSize)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null");
            }

            GenericObjectPool<T> pool = new GenericObjectPool<T>
            {
                prefab = prefab
            };
            pool.AddNewObjectsToPool(initialSize);
            return pool;
        }

        public T GetObject()
        {
            if (spawnedObjects.Count == 0)
            {
                AddNewObjectsToPool(1);
            }

            T pooledObject = spawnedObjects.Pop();
            pooledObject.gameObject.SetActive(true);
            return pooledObject;
        }

        private void AddNewObjectsToPool(int objectCount)
        {
            for (int i = 0; i < objectCount; ++i)
            {
                T pooledObject = UnityEngine.Object.Instantiate(prefab);
                pooledObject.gameObject.SetActive(false);
                spawnedObjects.Push(pooledObject);
            }
        }

        public void ReturnObjectToPool(T pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
            spawnedObjects.Push(pooledObject);
        }
    }
}