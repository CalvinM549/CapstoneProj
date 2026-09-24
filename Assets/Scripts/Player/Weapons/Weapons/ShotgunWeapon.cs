using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/Shotgun")]
public class ShotgunWeapon : PlayerWeapon
{
    public int pelletCount;
    public float spreadAngle;

    public override void Fire(Vector2 direction)
    {
        TimescaleManager.Instance.RequestTimeSlow(0.1f);

        p.Movement.PushPlayer(-direction, 20, 0.25f, true);

        for (int i = 0; i < pelletCount; i++)
        {
            float spread = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            Vector2 fireDir = Quaternion.Euler(0f, 0f, spread) * direction;

            FireSingle(fireDir);
        }

        currentAmmo--;
        ammoUsedEvent?.Invoke(currentAmmo);
    }

    private void FireSingle(Vector2 direction)
    {
        var projectile = ProjectilePools.FireProjectile(this, Projectiles[0], p.transform.position, direction);
    }
}
