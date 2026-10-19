using System;
using System.Collections.Generic;
using UnityEngine;

// A small passive bonus bought with shards in the hub and slotted into the loadout (one per run).
// Applied once when the run starts; stat entries use the same format as upgrades.
[CreateAssetMenu(menuName = "Run/Run Modifier", fileName = "Mod_")]
public class RunModifier : DatabaseEntry, IUnlockable
{
    [Header("Display")]
    public string modifierName = "New Protocol";
    [TextArea] public string flavourText;
    [TextArea] public string effectText;
    public Sprite icon;

    public string Name => modifierName;
    public string FlavourDescription => flavourText;
    public string EffectDescription => effectText;
    public Sprite Icon => icon;

    [Header("Meta Unlock")]
    public bool unlockedByDefault = false;
    public bool listInUnlocks = true;
    public int unlockCost = 50;

    public bool UnlockedByDefault => unlockedByDefault;
    public bool ListInUnlocks => listInUnlocks;
    public int UnlockCost => unlockCost;

    [Header("Effect")]
    public List<UpgradeStatModifierEntry> statModifiers = new();
    [Tooltip("Run currency granted when a new run starts (not on resume).")]
    public int startingCurrency;

    // Never appears as in-run loot
    public float BaseDropWeight => 0f;
    public IReadOnlyList<string> SynergyTags => Array.Empty<string>();
    public bool Buyable => false;
    public int ShopPrice => 0;

    public void Apply(Player player)
    {
        foreach (var entry in statModifiers)
            player.Stats.ApplyStatModifier(entry.statID, new StatModifier(entry.value, entry.modifierType, this));
    }
}
