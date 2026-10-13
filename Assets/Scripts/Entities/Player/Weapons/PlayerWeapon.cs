using System;
using System.Collections.Generic;
using UnityEngine;

public enum FireType
{
    Linked,
    Blocked,
    Independent
}

public class ActiveWeapon : IProjectileEmitter
{
    public readonly PlayerWeapon data;
    public readonly Player player;

    public int ammo;
    public int MaxAmmo => data.baseAmmo;
    public bool HasAmmo => ammo > 0;

    public event Action<int> AmmoChanged;

    // Interface
    public ProjectileData[] Projectiles => data.projectiles;
    public bool IsPlayerProjectile => true;
    public bool PoolRequested { get; set; }

    public ActiveWeapon(PlayerWeapon data, Player player, int ammoOverride = -1)
    {
        this.data = data;
        this.player = player;

        ammo = ammoOverride >= 0 ? ammoOverride : data.baseAmmo;
    }

    public void SpendAmmo(int used = 1)
    {
        ammo = Mathf.Max(0, ammo - used);
        AmmoChanged?.Invoke(ammo);
    }

    public void AddAmmo(int amount)
    {
        ammo = Mathf.Min(ammo + amount, MaxAmmo);
        AmmoChanged?.Invoke(ammo);
    }
}

public abstract class PlayerWeapon : DatabaseEntry, ILootEntry
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

    public ProjectileData[] projectiles;

    [Header("Loot")]
    public float baseDropRate = 1f;
    public string[] synergyTags = Array.Empty<string>();


    public bool spawnInShops = true;
    public int shopPrice = 50;

    public bool Buyable => spawnInShops;
    public int ShopPrice => shopPrice;

    public float BaseDropWeight => baseDropRate;
    public IReadOnlyList<string> SynergyTags => synergyTags;

    public virtual void OnEquip(ActiveWeapon instance) { }
    public virtual void OnUnequip(ActiveWeapon instance) { }
    public virtual void UpdateWeapon(ActiveWeapon instance) { }
    public virtual bool CanFire(ActiveWeapon instance) 
    {
        if (instance.HasAmmo)
            return true;
        
        GameEvents.AmmoUsedEmpty();
        return false;
    }
    public abstract void Fire(ActiveWeapon instance, Vector2 direction, Vector2 firePoint);

    protected virtual HitData BuildOutgoingHit(ActiveWeapon instance, ProjectileData projectile,
        Vector2 direction, Vector2 firePoint)
    {
        Player p = instance.player;

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

        return hitData;
    }

    protected virtual void FireSingle(ActiveWeapon instance, ProjectileData projectile, Vector2 direction,
        Vector2 firePoint, bool useAmmo = true)
    {
        var hitData = BuildOutgoingHit(instance, projectile, direction, firePoint);

        ProjectilePools.FireProjectile(instance, projectile, hitData, firePoint, direction);

        if (useAmmo)
            instance.SpendAmmo();
    }
}
