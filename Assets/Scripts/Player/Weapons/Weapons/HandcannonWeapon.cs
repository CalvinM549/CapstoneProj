using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/HandCannon")]
public class HandcannonWeapon : PlayerWeapon
{
    public override void Fire(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        VFXManager.Instance.PlayVFX(VFXType.ShootIntense, p.transform.position, Quaternion.Euler(0f, 0f, angle));
        CameraManager.Instance.CameraShake(1f);
        TimescaleManager.Instance.RequestTimeSlow(0.2f);

        p.Movement.PushPlayer(-direction, 15, 0.25f, true);

        var projectile = ProjectilePools.FireProjectile(this, Projectiles[0], p.transform.position, direction);

        currentAmmo--;
        ammoUsedEvent?.Invoke(currentAmmo);
    }
}
