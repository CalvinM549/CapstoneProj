using UnityEngine;

public enum FireType
{
    Linked,
    Blocked,
    Independent
}

public abstract class PlayerWeapon : ScriptableObject, IProjectileEmitter
{
    public string weaponName;
    public float cooldown = 1f;

    public FireType fireType;
    public bool automatic;

    public float windupTime;
    public float activeTime;
    public float recoveryTime;

    [SerializeField] private ProjectileData[] projectiles;

    public ProjectileData[] Projectiles => projectiles;
    public bool IsPlayerProjectile => true;
    public bool PoolRequested { get; set; }

    protected Player p;

    public virtual void OnEquip(Player p) { this.p = p; }
    public virtual void OnUnequip() { p = null; }
    public virtual void UpdateWeapon() { }

    public abstract bool CanFire();
    public abstract void Fire(Vector2 direction);
}
