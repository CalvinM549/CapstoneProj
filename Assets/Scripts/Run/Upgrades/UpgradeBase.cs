using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeSlot
{
    Frame,
    Weapons,
    Brain,
    Propulsion,
    None
}

public class UpgradeBase : DatabaseEntry, ICategorizedEntry<UpgradeSlot>, ILootEntry
{
    [Header("Display")]
    public string upgradeName = "New Upgrade";
    [TextArea] public string flavourText;
    [TextArea] public string effectText;
    public Sprite icon;

    public string Name => upgradeName;
    public string FlavourDescription => flavourText;
    public string EffectDescription => effectText;
    public Sprite Icon => icon;

    public UpgradeSlot slot = UpgradeSlot.None;

    [Header("Loot")]
    public float baseDropRate = 1f;
    public string[] synergyTags = Array.Empty<string>();

    public bool spawnInShops = true;
    public int shopPrice = 50;

    public float BaseDropWeight => baseDropRate;
    public IReadOnlyList<string> SynergyTags => synergyTags;
    public UpgradeSlot Category => slot;


    public bool Buyable => spawnInShops;
    public int ShopPrice => shopPrice;


    [Header("Effects")]
    protected Player player { get; private set; }


    public int maxStacks = 1;

    public List<UpgradeStatModifierEntry> statModifiers = new();


    public virtual void Apply(Player player)
    {
        this.player = player;
        ApplyStatModifiers(player.Stats);
        OnApply(player);
    }

    public virtual void Remove(Player player)
    {
        RemoveStatModifiers(player.Stats);
        OnRemove(player);
        this.player = null;
    }

    // Inheritance Things

    protected virtual void OnApply(Player player) { }

    protected virtual void OnRemove(Player player) { }

    #region Helpers

    public virtual void ModifyOutgoingHit(HitData hit) { }
    public virtual void ModifyIncomingHit(HitData hit) { }

    // Like ModifyOutgoingHit but runs as the hit lands, so the target is known (lock-on, target health, statuses)
    public virtual void ModifyHitOnTarget(HitData hit, EnemyController target) { }

    // Return true to stop a whiffed attack of this type from draining momentum
    public virtual bool PreventsWhiffDrain(AttackType type) => false;

    // Builds a player-owned melee hit for upgrades that deal their own damage (shockwaves, rams...).
    // Applies global + melee damage stats and runs the build's outgoing-hit modifiers, like a normal attack.
    protected HitData BuildPlayerMeleeHit(float damage, AttackType type, Vector2 knockbackDirection, float knockback, float hitstun)
    {
        var hit = new HitData(damage, type, true)
        {
            sourcePos = player.transform.position,
            knockbackDirection = knockbackDirection,
            knockbackForce = knockback,
            hitstunTime = hitstun,
            hitstopTime = 0f,
            isParryable = false
        };

        hit.AddModifier(player.Stats.Get(StatRef.PlayerOutgoingDamageMult) - 1f, StatModType.PercentAdd, this);
        hit.AddModifier(player.Stats.Get(StatRef.PlayerMeleeDamageMult) - 1f, StatModType.PercentAdd, this);
        player.Upgrades.ModifyOutgoingHit(hit);

        return hit;
    }

    protected int Stacks => this is AuxUpgrade aux && player != null ? Mathf.Max(1, player.Upgrades.GetAuxStacks(aux)) : 1;

    protected void ApplyStatModifiers(PlayerStats stats)
    {
        foreach (var entry in statModifiers)
        {
            var mod = new StatModifier(entry.value, entry.modifierType, this);
            stats.ApplyStatModifier(entry.statID, mod);
        }
    }

    protected void RemoveStatModifiers(PlayerStats stats)
    {
        stats.RemoveModifiersFromSource(this);
    }

    #endregion
}

[Serializable]
public class UpgradeStatModifierEntry
{
    public string statID;
    public float value;
    public StatModType modifierType = StatModType.Flat;
}