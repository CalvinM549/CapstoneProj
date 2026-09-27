using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/HandCannon")]
public class HandcannonWeapon : PlayerWeapon
{
    public override void Fire(Vector2 direction, Vector2 firePoint)
    {
        VFXManager.Instance.PlayVFX(VFXType.ShootIntense, firePoint, direction);
        CameraManager.Instance.CameraShake(1f);
        TimescaleManager.Instance.RequestTimeSlow(0.2f);

        p.Movement.PushPlayer(-direction, 15, 0.25f, true);

        FireSingle(Projectiles[0], direction, firePoint, useAmmo: true);
    }
}
