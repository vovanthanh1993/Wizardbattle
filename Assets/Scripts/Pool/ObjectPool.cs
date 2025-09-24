using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private T _prefab;
    private List<T> _pool;
    private Transform _parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;
        _pool = new List<T>();

        // Pre-populate pool
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    private T CreateNewObject()
    {
        T obj = Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(false);
        _pool.Add(obj);
        
        // Reset state if object has ResetState method
        var resetMethod = obj.GetType().GetMethod("ResetState");
        if (resetMethod != null)
        {
            resetMethod.Invoke(obj, null);
        }
        
        return obj;
    }

    public T Get()
    {
        // Find inactive object in pool
        for (int i = _pool.Count - 1; i >= 0; i--)
        {
            // Check if object still exists
            if (_pool[i] == null)
            {
                _pool.RemoveAt(i);
                continue;
            }
            
            if (!_pool[i].gameObject.activeInHierarchy)
            {
                _pool[i].gameObject.SetActive(true);
                
                // Reset state if object has ResetState method
                var resetMethod = _pool[i].GetType().GetMethod("ResetState");
                if (resetMethod != null)
                {
                    resetMethod.Invoke(_pool[i], null);
                }
                
                return _pool[i];
            }
        }

        // If no inactive object found, create new one
        return CreateNewObject();
    }

    public void Return(T obj)
    {
        if (obj != null && obj.gameObject != null)
        {
            obj.gameObject.SetActive(false);
        }
    }

    public void ReturnAll()
    {
        CleanupDestroyedObjects();
        for (int i = 0; i < _pool.Count; i++)
        {
            if (_pool[i] != null && _pool[i].gameObject != null)
            {
                _pool[i].gameObject.SetActive(false);
            }
        }
    }
    
    // Get all active objects in the pool
    public List<T> GetActiveObjects()
    {
        CleanupDestroyedObjects();
        List<T> activeObjects = new List<T>();
        for (int i = 0; i < _pool.Count; i++)
        {
            if (_pool[i] != null && _pool[i].gameObject != null && _pool[i].gameObject.activeInHierarchy)
            {
                activeObjects.Add(_pool[i]);
            }
        }
        return activeObjects;
    }
    
    // Get all objects in the pool (active and inactive)
    public List<T> GetAllObjects()
    {
        return new List<T>(_pool);
    }
    
    // Cleanup destroyed objects from pool
    public void CleanupDestroyedObjects()
    {
        for (int i = _pool.Count - 1; i >= 0; i--)
        {
            if (_pool[i] == null)
            {
                _pool.RemoveAt(i);
            }
        }
    }
    
    // Get pool statistics
    public int GetPoolSize()
    {
        CleanupDestroyedObjects();
        return _pool.Count;
    }
    
    public int GetActiveObjectsCount()
    {
        CleanupDestroyedObjects();
        int count = 0;
        for (int i = 0; i < _pool.Count; i++)
        {
            if (_pool[i] != null && _pool[i].gameObject.activeInHierarchy)
            {
                count++;
            }
        }
        return count;
    }
}
