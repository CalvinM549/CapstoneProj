using Mono.Cecil;
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

public class UpgradeBase : ScriptableObject
{
    [Header("Display")]
    public string upgradeName = "New Upgrade";
    public string upgradeID = "";
    [TextArea] public string flavourText;
    [TextArea] public string effectText;

    public Sprite icon;
    // Rarity?

    public UpgradeSlot slot         = UpgradeSlot.None;
    public UpgradeCategory[] categories;

    public int maxStacks = 1;

    protected Player player { get; private set; }

    // Upgrade effects

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

public class UpgradeStatModifierEntry
{
    public string statID;
    public float value;
    public StatModType modifierType = StatModType.Flat;
}