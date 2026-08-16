using System;
using UnityEngine;

public enum FireType
{
    Linked,
    Blocked,
    Independent
}

public abstract class PlayerWeapon : DatabaseEntry, IProjectileEmitter
{
    public string weaponName;
    public float cooldown = 1f;
    public int baseAmmo;
    [HideInInspector] public int currentAmmo;

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
    protected Action<int> ammoUsedEvent;

    public virtual void OnEquip(Player p, Action<int> ammoUseEvent)
    {
        this.p = p;
        currentAmmo = baseAmmo;
        ammoUsedEvent = ammoUseEvent;

        ammoUsedEvent?.Invoke(currentAmmo);
    }
    public virtual void OnUnequip()
    {
        p = null;
        ammoUsedEvent = null;
    }
    public virtual void UpdateWeapon() { }
    public virtual bool CanFire() 
    { 
        if (currentAmmo > 0)
        {
            return true;
        }
        else
        {
            GameEvents.AmmoUsedEmpty();
            return false;
        }

    }
    public abstract void Fire(Vector2 direction);
}
