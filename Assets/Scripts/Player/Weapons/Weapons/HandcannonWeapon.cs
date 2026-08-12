using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/HandCannon")]
public class HandcannonWeapon : PlayerWeapon
{
    public override void Fire(Vector2 direction)
    {
        VFXManager.Instance.PlayVFX(VFXType.ShootIntense, p.transform.position, direction);
        CameraManager.Instance.CameraShake(1f);
        TimescaleManager.Instance.RequestTimeSlow(0.2f);

        p.Movement.PushPlayer(-direction, 15, 0.25f, true);

        var projectile = ProjectilePools.FireProjectile(this, Projectiles[0], p.transform.position, direction);

        currentAmmo--;
        ammoUsedEvent?.Invoke(currentAmmo);
    }
}
