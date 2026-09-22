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


    public void Apply(Player player)
    {
        this.player = player;
        ApplyStatModifiers(player.Stats);
        OnApply(player);
    }

    public void Remove(Player player)
    {
        RemoveStatModifiers(player.Stats);
        OnRemove(player);
        player = null;
    }

    // Inheritance Things

    protected virtual void OnApply(Player player) { }

    protected virtual void OnRemove(Player player) { }

    #region Helpers

    private void ApplyStatModifiers(PlayerStats stats)
    {
        foreach (var entry in statModifiers)
        {
            var mod = new StatModifier(entry.value, entry.modifierType, this);
            stats.ApplyStatModifier(entry.statID, mod);
        }
    }

    private void RemoveStatModifiers(PlayerStats stats)
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