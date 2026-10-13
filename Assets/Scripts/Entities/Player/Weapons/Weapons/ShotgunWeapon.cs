using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/Shotgun")]
public class ShotgunWeapon : PlayerWeapon
{
    public int pelletCount;
    public float spreadAngle;

    public override void Fire(ActiveWeapon instance, Vector2 direction, Vector2 firePoint)
    {
        VFXManager.Instance.PlayVFX(VFXType.ShootIntense, firePoint, direction);
        CameraManager.Instance.CameraShake(1f);
        TimescaleManager.Instance.RequestHitStop(0.1f);

        instance.player.Movement.PushPlayer(-direction, 20, 0.25f, true);

        for (int i = 0; i < pelletCount; i++)
        {
            float spread = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
            Vector2 fireDir = Quaternion.Euler(0f, 0f, spread) * direction;

            FireSingle(instance, projectiles[0], fireDir, firePoint, useAmmo: false);
        }

        instance.SpendAmmo();
    }
}
