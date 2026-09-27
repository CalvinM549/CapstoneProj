using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/MissileLauncher")]
public class MissileWeapon : PlayerWeapon
{
    public int burstCount = 1;
    public float burstSpread;
    public float burstDelay;

    private Transform currentTarget;

    public override void Fire(Vector2 direction, Vector2 firePoint)
    {
        currentTarget = p.Targeting.HasTarget ? p.Targeting.LockedTarget : null;
        
        p.StartCoroutine(BurstRoutine(firePoint));
    }

    private IEnumerator BurstRoutine(Vector2 firePoint)
    {
        for (int i = 0; i < burstCount; i++)
        {
            if (currentAmmo <= 0) yield break;

            Vector2 direction = p.GetTargetDirection();
            if (i % 2 == 0)
                direction = new Vector2(direction.y, -direction.x);
            else
                direction = new Vector2(-direction.y, direction.x);

            VFXManager.Instance.PlayVFX(VFXType.ShootIntense, firePoint, direction);
            CameraManager.Instance.CameraShake(0.5f);

            FireSingle(Projectiles[0], direction, firePoint, useAmmo: true);

            if(i < burstCount - 1)
                yield return new WaitForSeconds(burstDelay);
        }
    }


    protected override void FireSingle(ProjectileData projectile, Vector2 direction, Vector2 firePoint, bool useAmmo = true)
    {
        var hitData = new HitData(projectile.damage, AttackType.Projectile, isPlayerAttack: true)
        {
            sourcePos = firePoint,
            knockbackDirection = direction,
            knockbackForce = projectile.knockback,

            hitstopTime = projectile.hitstopDuration,
            hitstunTime = projectile.hitstunTime,

            isParryable = projectile.isParryable,
            isBlockable = projectile.isBlockable
        };

        hitData.AddModifier(p.Stats.Get(StatRef.PlayerOutgoingDamageMult) - 1f, StatModType.PercentAdd, p.Combat);
        hitData.AddModifier(p.Stats.Get(StatRef.PlayerRangedDamageMult) - 1f, StatModType.PercentAdd, p.Combat);

        p.Upgrades.ModifyOutgoingHit(hitData);

        ProjectilePools.FireHomingProjectile(this, projectile, hitData, firePoint, direction, currentTarget);

        if (useAmmo)
        {
            currentAmmo--;
            ammoUsedEvent?.Invoke(currentAmmo);
        }
    }
}
