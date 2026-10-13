using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/MissileLauncher")]
public class MissileWeapon : PlayerWeapon
{
    public int burstCount = 1;
    public float burstSpread;
    public float burstDelay;

    public override void Fire(ActiveWeapon instance, Vector2 direction, Vector2 firePoint)
    {
        Player p = instance.player;

        Transform target = p.Targeting.HasTarget ? p.Targeting.LockedTarget : null;
        
        p.StartCoroutine(BurstRoutine(instance, firePoint, target));
    }

    private IEnumerator BurstRoutine(ActiveWeapon instance, Vector2 firePoint, Transform target)
    {
        Player p = instance.player;

        for (int i = 0; i < burstCount; i++)
        {
            if (!instance.HasAmmo) yield break;
            if (p.Combat.EquippedWeapon != instance) yield break;

            Vector2 direction = p.GetTargetDirection();
            if (i % 2 == 0)
                direction = new Vector2(direction.y, -direction.x);
            else
                direction = new Vector2(-direction.y, direction.x);

            VFXManager.Instance.PlayVFX(VFXType.ShootIntense, firePoint, direction);
            CameraManager.Instance.CameraShake(0.5f);

            FireHoming(instance, projectiles[0], direction, firePoint, target);

            if(i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }
    }


    private void FireHoming(ActiveWeapon instance, ProjectileData projectile, Vector2 direction, Vector2 firePoint, Transform target)
    {
        var hitData = BuildOutgoingHit(instance, projectile, direction, firePoint);

        ProjectilePools.FireHomingProjectile(instance, projectile, hitData, firePoint, direction, target);

        instance.SpendAmmo();
    }
}
