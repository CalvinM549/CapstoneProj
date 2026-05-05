using UnityEngine;

[CreateAssetMenu(fileName = "ShotgunTool", menuName = "Player/Tools/ShotgunTool")]
public class ShotgunTool : PlayerTool, IProjectileEmitter
{
    public int pelletCount; // number of pellets fired per shot
    public float spreadAngle; // angle of spread for the pellets

    public int shotsBeforeCooldown;

    // Projectile Emitter
    public ProjectileData pelletData;
    public ProjectileData[] Projectiles => new ProjectileData[] { pelletData };
    public bool IsPlayerProjectile => true;
    public bool PoolRequested { get; set; }

    // Runtime
    private int shotsFired = 0;
    private Transform firePoint;

    public override void OnEquip(Player player)
    {
        firePoint = player.transform.Find("FirePoint") ?? player.transform;
        ProjectilePools.RequestPool(this);
        shotsFired = 0;
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        ProjectilePools.ReleasePool(this);
        shotsFired = 0;
    }

    protected override bool OnUse(Vector2 direction)
    {
        if (pelletData == null) return false;

        FirePellets(direction);
        shotsFired++;

        if (shotsFired >= shotsBeforeCooldown)
        {
            shotsFired = 0;
            return true;
        }

        return false;
    }

    protected override void OnReset()
    {
        shotsFired = 0;
    }

    private void FirePellets(Vector2 direction)
    {
        for (int i = 0; i < pelletCount; i++)
        {
            float spread = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            Vector2 fireDir = Quaternion.Euler(0f, 0f, spread) * direction;
            FireProjectile(pelletData, firePoint.position, fireDir, true);
        }
    }

    public void FireProjectile(ProjectileData projectile, Vector2 firePos, Vector2 direction, bool isPlayer)
    {
        ProjectileManager.Instance.FireProjectile(projectile, firePos, direction, isPlayer);
    }   
}
