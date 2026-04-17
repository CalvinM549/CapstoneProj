using UnityEngine;

public interface IProjectileEmitter
{
    ProjectileData[] Projectiles {  get; }
    bool IsPlayerProjectile { get; }
    bool PoolRequested { get; set; }

    void FireProjectile(ProjectileData projectile, Vector2 firePos, Vector2 direction, bool isPlayer);
}

public static class ProjectilePools
{
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
}
