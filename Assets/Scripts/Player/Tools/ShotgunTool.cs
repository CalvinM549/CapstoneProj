using UnityEngine;

public class ShotgunTool : PlayerTool, IProjectileEmitter
{
    [SerializeField] private int pelletCount; // number of pellets fired per shot
    [SerializeField] private float spreadAngle; // angle of spread for the pellets

    [SerializeField] private float cooldown;
    [SerializeField] private int shotsBeforeCooldown;

    [SerializeField] private Transform firePoint;

    private int shotsFired = 0;
    private float cooldownTimer = 0f;

    public ProjectileData[] Projectiles => new ProjectileData[0]; // Define your projectile data here
    public bool IsPlayerProjectile => true;
    public bool PoolRequested { get; set; }

    public override bool UseTool(Vector2 direction)
    {
        if (cooldownTimer > 0f) return false;

        for (int i = 0; i < pelletCount; i++)
        {
            float spread = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            Vector2 fireDir = Quaternion.Euler(0f, 0f, spread) * direction;

            FireProjectile(Projectiles[0], firePoint.position, fireDir, true);
        }

        // Muzzle flash, sound effects, etc.

        shotsFired++;
        if (shotsFired >= shotsBeforeCooldown)
        {
            cooldownTimer = cooldown;
            shotsFired = 0;
        }

        return true; // Return true if the tool was successfully used
    }

    public override bool TryIntercept(HitData incoming) => false;

    public override void OnEquip(Transform currentTransform)
    {
        playerTransform = currentTransform;
        ProjectilePools.RequestPool(this);
    }

    public override void OnUnequip()
    {
        ProjectilePools.ReleasePool(this);
        // Optional: Add any cleanup logic when the tool is unequipped
    }

    public override void UpdateTool()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public void FireProjectile(ProjectileData projectile, Vector2 firePos, Vector2 direction, bool isPlayer)
    {
        ProjectileManager.Instance.FireProjectile(projectile, firePos, direction, isPlayer);
    }   
}
