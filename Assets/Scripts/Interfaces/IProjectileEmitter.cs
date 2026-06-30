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

        if (ProjectileManager.Instance == null)
        {
            Debug.LogError("NO PROJECTILE MANAGER IN SCENE");
            return;
        }

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

    // Firing

    public static Projectile FireProjectile(IProjectileEmitter emitter, ProjectileData data, Vector2 firePos, Vector2 direction)
    {
        if (!emitter.PoolRequested)
        {
            Debug.LogWarning($"[ProjectilePools] Fire called before pool requested on {data.name}");
            return null;
        }

        return ProjectileManager.Instance.FireProjectile(data, firePos, direction, emitter.IsPlayerProjectile);
    }

    //public static HomingProjectile FireHomingProjectile() 
}
