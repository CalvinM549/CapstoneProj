using System;
using System.Collections.Generic;
using UnityEngine;

public enum FireType
{
    Linked,
    Blocked,
    Independent
}

public abstract class PlayerWeapon : DatabaseEntry, ILootEntry, IProjectileEmitter
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


    public bool spawnInShops = true;
    public int shopPrice = 50;

    public bool Buyable => spawnInShops;
    public int ShopPrice => shopPrice;

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
    public abstract void Fire(Vector2 direction, Vector2 firePoint);

    protected virtual void FireSingle(ProjectileData projectile, Vector2 direction, Vector2 firePoint, bool useAmmo = true)
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

        ProjectilePools.FireProjectile(this, projectile, hitData, firePoint, direction);

        if (useAmmo)
        {
            currentAmmo--;
            ammoUsedEvent?.Invoke(currentAmmo);
        }
    }
}
