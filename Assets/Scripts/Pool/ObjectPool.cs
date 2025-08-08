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
        for (int i = 0; i < _pool.Count; i++)
        {
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
        if (obj != null)
        {
            obj.gameObject.SetActive(false);
        }
    }

    public void ReturnAll()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            if (_pool[i] != null)
            {
                _pool[i].gameObject.SetActive(false);
            }
        }
    }
}
