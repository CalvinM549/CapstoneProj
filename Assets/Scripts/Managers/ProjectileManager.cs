using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

// Handles projectile pools and firing projectiles
// KEEP ONE PER SCENE TO CLEAN BETWEEN FLOORS
public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance;

    private class PoolEntry
    {
        public ObjectPool<Projectile> pool;
        public int userCount;
    }

    private Dictionary<ProjectileData, PoolEntry> activePools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RequestPool(ProjectileData projectile)
    {
        if (activePools.TryGetValue(projectile, out var poolEntry))
        {
            poolEntry.userCount++;
            return;
        }

        activePools[projectile] = new PoolEntry()
        {
            pool = new ObjectPool<Projectile>(projectile.prefab, projectile.poolSize, transform),
            userCount = 1
        };

    }

    public void ReleasePool(ProjectileData projectile)
    {
        if (!activePools.TryGetValue(projectile, out var poolEntry))
            return;

        poolEntry.userCount--;
        if (poolEntry.userCount <= 0)
        {
            poolEntry.pool.Clear();

            activePools.Remove(projectile);
        }
    }

    public Projectile FireProjectile(
        ProjectileData projectile,
        HitData hitData,
        Vector2 firePos, 
        Vector2 direction, 
        bool isPlayerProjectile = false)
    {
        if (!activePools.TryGetValue(projectile, out var poolEntry))
        {
            Debug.LogError($"[ProjectileManager] No Pool found for projectile type {projectile.name}");
            return null;
        }

        Projectile currentProjectile = poolEntry.pool.Get();
        currentProjectile.transform.position = firePos;
        currentProjectile.Initialize(projectile, hitData, direction, firePos, ReturnToPool, isPlayerProjectile);

        return currentProjectile;
    }

    private void ReturnToPool(Projectile projectile)
    {
        if (activePools.TryGetValue(projectile.data, out var poolEntry))
            poolEntry.pool.ReturnToPool(projectile);

        else
            projectile.gameObject.SetActive(false);
    }

    public void ClearAllPools()
    {
        foreach(var entry in activePools.Values)
            entry.pool.Clear();

        activePools.Clear();
    }
}
