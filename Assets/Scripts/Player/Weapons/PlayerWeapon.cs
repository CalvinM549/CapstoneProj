using UnityEngine;

public enum FireType
{
    Linked,
    Independent
}

public abstract class PlayerWeapon : ScriptableObject, IProjectileEmitter
{
    public string weaponName;
    public float cooldown = 1f;

    public FireType fireType;
    public bool automatic;

    public ProjectileData[] Projectiles { get; }
    public bool IsPlayerProjectile => true;
    public bool PoolRequested { get; set; }

    public virtual void OnEquip(Player p) { }
    public virtual void OnUnequip() { }
    public virtual void UpdateWeapon() { }

    public abstract bool CanFire();
    public abstract void Fire();
}
