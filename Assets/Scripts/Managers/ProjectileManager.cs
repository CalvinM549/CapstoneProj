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
            Debug.Log($"[ProjectileManager] {projectile.name} user added");
            return;
        }

        activePools[projectile] = new PoolEntry()
        {
            pool = new ObjectPool<Projectile>(projectile.prefab, projectile.poolSize, transform),
            userCount = 1
        };

        Debug.Log($"[ProjectileManager] {projectile.name} pool created");
    }

    public void ReleasePool(ProjectileData projectile)
    {
        if (!activePools.TryGetValue(projectile, out var poolEntry))
            return;

        poolEntry.userCount--;
        if (poolEntry.userCount <= 0)
        {
            activePools.Remove(projectile);
            Debug.Log($"[ProjectileManager] {projectile.name} pool removed");
        }
    }

    public Projectile FireProjectile(
        ProjectileData projectile,
        Vector2 firePos, 
        Vector2 direction, 
        bool isPlayerProjectile = false)
    {
        if (!activePools.TryGetValue(projectile, out var poolEntry))
            return null;

        Projectile currentProjectile = poolEntry.pool.Get();
        currentProjectile.transform.position = firePos;
        currentProjectile.Initalize(projectile, direction, firePos, ReturnToPool, isPlayerProjectile);

        return currentProjectile;
    }

    private void ReturnToPool(Projectile projectile)
    {
        if (activePools.TryGetValue(projectile.data, out var poolEntry))
            poolEntry.pool.ReturnToPool(projectile);

        else
            projectile.gameObject.SetActive(false);
    }
}
