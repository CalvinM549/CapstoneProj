using UnityEngine;

public interface IProjectileEmitter
{
    ProjectileData[] Projectiles {  get; }
    bool IsPlayerProjectile { get; }
    bool PoolRequested { get; set; }
}

public static class ProjectilePools
{
    // Poolers

    public static void RequestPool(IProjectileEmitter emitter)
    {
        if (emitter.PoolRequested) return;
        if (ProjectileManager.Instance == null) return;

        foreach (var projectile in emitter.Projectiles)
            ProjectileManager.Instance.RequestPool(projectile);

        emitter.PoolRequested = true;
    }

    public static void ReleasePool(IProjectileEmitter emitter)
    {
        if (!emitter.PoolRequested) return;

        foreach(var projectile in emitter.Projectiles)
            ProjectileManager.Instance.ReleasePool(projectile);

        emitter.PoolRequested = false;
    }

    #region Firing

    public static Projectile FireProjectile(
        IProjectileEmitter emitter,
        ProjectileData data, 
        HitData hitData,
        Vector2 firePos,
        Vector2 direction)
    {
        if (!emitter.PoolRequested)
        {
            Debug.LogWarning($"[ProjectilePools] Fire called before pool requested on {data.name}");
            return null;
        }

        return ProjectileManager.Instance.FireProjectile(data, hitData, firePos, direction, emitter.IsPlayerProjectile);
    }

    public static HomingProjectile FireHomingProjectile(
        IProjectileEmitter emitter,
        ProjectileData data,
        HitData hitData,
        Vector2 firePos, 
        Vector2 direction,
        Transform homingTarget)
    {
        Projectile projectile = FireProjectile(emitter, data, hitData, firePos, direction);

        if (projectile is HomingProjectile homing)
        {
            homing.HomingTarget = homingTarget;
            return homing;
        }

        if (projectile == null)
            Debug.LogError("Null Projectile found");

        Debug.LogWarning($"[ProjectilePools] FireHomingProjectile called with non-homing ProjectileData '{data.name}'");
        return null;
    }

    #endregion
}
