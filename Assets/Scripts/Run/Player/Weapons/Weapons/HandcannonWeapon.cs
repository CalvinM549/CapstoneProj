using UnityEngine;

[CreateAssetMenu(menuName = "Player/Weapon/HandCannon")]
public class HandcannonWeapon : PlayerWeapon
{
    public override bool CanFire() => true;

    public override void Fire(Vector2 direction)
    {
        var projectile = ProjectilePools.FireProjectile(this, Projectiles[0], p.transform.position, direction);
    }
}
