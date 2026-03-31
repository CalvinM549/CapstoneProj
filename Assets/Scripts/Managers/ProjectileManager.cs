using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

// Handles projectile pools and firing projectiles
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
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RequestPool(ProjectileData projectile)
    {
        if (activePools.TryGetValue(projectile, out var poolEntry))
        {
            poolEntry.userCount++;
            Debug.Log("New Pool user");
            return;
        }

        activePools[projectile] = new PoolEntry()
        {
            pool = new ObjectPool<Projectile>(projectile.prefab, projectile.poolSize, transform),
            userCount = 1
        };

        Debug.Log("New Pool Created");
    }

    public void ReleasePool(ProjectileData projectile)
    {
        if (!activePools.TryGetValue(projectile, out var poolEntry))
            return;

        poolEntry.userCount--;
        if (poolEntry.userCount <= 0)
        {
            activePools.Remove(projectile);
            Debug.Log("Pool removed");
        }
    }

    public void FireProjectile(ProjectileData projectile, Vector2 firePos, Vector2 direction, bool isPlayerProjectile = false)
    {
        if (!activePools.TryGetValue(projectile, out var poolEntry))
            return;

        Projectile currentProjectile = poolEntry.pool.Get();
        currentProjectile.transform.position = firePos;
        currentProjectile.Initalize(projectile, direction, ReturnToPool, isPlayerProjectile);
    }

    private void ReturnToPool(Projectile projectile)
    {
        if (activePools.TryGetValue(projectile.data, out var poolEntry))
            poolEntry.pool.ReturnToPool(projectile);

        else
            projectile.gameObject.SetActive(false);
    }
}
