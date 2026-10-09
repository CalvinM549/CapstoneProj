using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Stack<T> pool = new();

    private readonly List<T> allInstances = new();

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
        pool.Push(obj);
        allInstances.Add(obj);
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

        var obj = pool.Pop();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnToPool(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Push(obj);
    }

    public void Clear()
    {
        foreach (var obj in allInstances)
        {
            if(obj != null)
                GameObject.Destroy(obj.gameObject);
        }

        allInstances.Clear();
        pool.Clear();

        liveCount = 0;
    }
}