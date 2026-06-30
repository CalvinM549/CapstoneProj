using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private T prefab;
    private Transform parent;
    private Queue<T> pool = new();
    private int liveCount;
    private readonly int maxSize;

    public ObjectPool(T prefab, int initialSize, Transform parent, int maxSize = -1)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.maxSize = maxSize;

        for (int i = 0; i < initialSize; i++)
        {
            CreateToPool();
        }
    }

    private void CreateToPool()
    {
        var obj = GameObject.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
        liveCount++;
    }

    public T Get()
    {
        if (pool.Count <= 0)
        {
            if (maxSize > 0 && liveCount >= maxSize)
                return null; // Ignores spawning new instance due to cap reached
            
            CreateToPool();
        }

        var obj = pool.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnToPool(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}