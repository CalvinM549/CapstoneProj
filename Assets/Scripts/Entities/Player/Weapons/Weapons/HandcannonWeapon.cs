using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/HandCannon")]
public class HandcannonWeapon : PlayerWeapon
{
    public override void Fire(ActiveWeapon instance, Vector2 direction, Vector2 firePoint)
    {
        VFXManager.Instance.PlayVFX(VFXType.ShootIntense, firePoint, direction);
        CameraManager.Instance.CameraShake(1f);
        TimescaleManager.Instance.RequestHitStop(0.2f);

        instance.player.Movement.PushPlayer(-direction, 15, 0.25f, true);

        FireSingle(instance, projectiles[0], direction, firePoint, useAmmo: true);


    }
}
