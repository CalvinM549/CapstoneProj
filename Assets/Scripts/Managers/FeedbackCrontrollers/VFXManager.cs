using System;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Serializable]
    private class VFXEntry
    {
        public VFXType type;
        public PooledVFX prefab;
        public int initialPoolSize = 10;
        public int maxPoolSize = -1;
    }

    [SerializeField] private VFXEntry[] vfxEntries;
    [SerializeField] private Transform vfxContainer;

    private Dictionary<VFXType, ObjectPool<PooledVFX>> pools = new();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        BuildPools();
    }

    private void BuildPools()
    {
        foreach (var entry in vfxEntries)
        {
            if (entry.prefab == null)
            {
                Debug.LogWarning($"[VFXManager] No prefab assigned to {entry.type}");
                continue;
            }

            if (pools.ContainsKey(entry.type))
            {
                Debug.LogWarning($"[VFXManager] Duplicate entry for {entry.type}");
                continue;
            }

            pools[entry.type] = new ObjectPool<PooledVFX>(
                entry.prefab, 
                entry.initialPoolSize, 
                vfxContainer != null ? vfxContainer : transform, 
                entry.maxPoolSize);
        }
    }

    public void PlayVFX(VFXType type, Vector3 position, Quaternion rotation = default)
    {
        if (!pools.TryGetValue(type, out var pool))
        {
            Debug.LogWarning($"[VFXManager] no pool registered for {type}");
            return;
        }

        var instance = pool.Get();
        if (instance == null)
            return;

        instance.Play(pool, position, rotation == default ? Quaternion.identity : rotation);
    }

    public void PlayVFX(VFXType type, Vector2 position) => PlayVFX(type, position, Quaternion.identity);
}
