using System;
using System.Collections.Generic;
using UnityEngine;

public enum FireType
{
    Linked,
    Blocked,
    Independent
}

public abstract class PlayerWeapon : DatabaseEntry, IRollableLoot, IProjectileEmitter
{
    [Header("Display")]
    public string weaponName = "New Weapon";
    [TextArea] public string flavourText;
    [TextArea] public string effectText;
    public Sprite icon;

    public string Name => weaponName;
    public string FlavourDescription => flavourText;
    public string EffectDescription => effectText;
    public Sprite Icon => icon;

    [Header("Effects")]

    public float cooldown = 1f;
    public int baseAmmo;

    public FireType fireType;
    public bool automatic;

    public float windupTime;
    public float activeTime;
    public float recoveryTime;

    // Projectile Emitter
    [SerializeField] private ProjectileData[] projectiles;

    public ProjectileData[] Projectiles => projectiles;
    public bool IsPlayerProjectile => true;
    public bool PoolRequested { get; set; }

    [Header("Loot")]
    public float baseDropRate = 1f;
    public string[] synergyTags = Array.Empty<string>();

    public float BaseDropWeight => baseDropRate;
    public IReadOnlyList<string> SynergyTags => synergyTags;

    // Functionals

    protected Player p;
    protected Action<int> ammoUsedEvent;
    [HideInInspector] public int currentAmmo;

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
