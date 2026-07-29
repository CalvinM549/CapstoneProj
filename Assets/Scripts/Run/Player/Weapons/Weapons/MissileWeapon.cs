using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/MissileLauncher")]
public class MissileWeapon : PlayerWeapon
{
    public int burstCount = 1;
    public float burstSpread;
    public float burstDelay;

    private Transform currentTarget;

    public override bool CanFire() => true;

    public override void Fire(Vector2 direction)
    {
        currentTarget = p.Targeting.HasTarget ? p.Targeting.LockedTarget : null;

        if (burstCount <= 1)
            FireSingle(direction);
        else
            p.StartCoroutine(BurstRoutine());
    }

    private IEnumerator BurstRoutine()
    {
        for (int i = 0; i < burstCount; i++)
        {
            Vector2 fireDirection = p.GetTargetDirection();
            if (i % 2 == 0)
                fireDirection = new Vector2(fireDirection.y, -fireDirection.x);
            else
                fireDirection = new Vector2(-fireDirection.y, fireDirection.x);

            FireSingle(fireDirection);

            if(i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }

    }

    private void FireSingle(Vector2 direction)
    {

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        VFXManager.Instance.PlayVFX(VFXType.ShootIntense, p.transform.position, Quaternion.Euler(0f, 0f, angle));
        CameraManager.Instance.CameraShake(0.5f);

        var projectile = ProjectilePools.FireProjectile(this, Projectiles[0], p.transform.position, direction);

        Debug.Log($"[MissileWeapon] Got projectile of type {projectile.GetType().Name}");

        if (projectile is not HomingProjectile homing)
        {
            Debug.Log($"[MissileWeapon] Projectile set is not a HomingProjectile type");
            return;
        }

        homing.HomingTarget = currentTarget;
    }
}
