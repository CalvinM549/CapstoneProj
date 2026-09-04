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

public enum UpgradeCategory
{
    General,
    Frame,
    Weapons,
    Brain,
    Movement
}

public class UpgradeBase : DatabaseEntry, ICategorizedEntry<UpgradeSlot>, IRollableLoot
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
    public int maxStacks = 1;

    [Header("Loot")]
    public float baseDropRate = 1f;
    public string[] synergyTags = Array.Empty<string>();

    public float BaseDropWeight => baseDropRate;
    public IReadOnlyList<string> SynergyTags => synergyTags;
    public UpgradeSlot Category => slot;

    [Header("Effects")]
    protected Player player { get; private set; }



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