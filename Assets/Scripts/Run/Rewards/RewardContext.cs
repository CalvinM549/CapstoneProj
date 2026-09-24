using System.Collections.Generic;
using UnityEngine;

public enum RewardCategory
{
    MajorUpgrade,
    AuxUpgrade,
    Weapon,
    Tool
}

public class RewardContext
{
    private readonly PlayerUpgrades upgrades;

    public HashSet<string> ownedWeaponIds = new();
    public HashSet<string> ownedToolIds = new();

    public HashSet<string> SeenItemIdsThisRun = new();
    public HashSet<string> ActiveSynergyTags = new();

    public Dictionary<RewardCategory, int> PityCounters = new();

    public RewardContext(PlayerUpgrades upgrades)
    {
        this.upgrades = upgrades;
    }

    public bool IsMajorSlotFilled(UpgradeSlot slot) => upgrades.GetMajor(slot) != null;
    public int GetAuxStacks(AuxUpgrade upgrade) => upgrades.GetAuxStacks(upgrade);

    public bool IsOwnedAtMax(ILootEntry entry)
    {
        switch (entry)
        {
            case AuxUpgrade upgrade:
                return upgrades.GetAuxStacks(upgrade) >= upgrade.maxStacks;

            default:
                return false;
        }
    }

    public int GetPity(RewardCategory category) => PityCounters.TryGetValue(category, out var pity) ? pity : 0;

    public void ResetPity(RewardCategory category) => PityCounters[category] = 0;

    public void IncrementPityExcept(RewardCategory category)
    {
        foreach (RewardCategory c in System.Enum.GetValues(typeof(RewardCategory)))
        {
            if (c == category) continue;
            PityCounters[category]++;
        }
    }

    public void RegisterOffer(RewardCategory category, List<LootResult> result)
    {
        ResetPity(category);
        IncrementPityExcept(category);
    }
}
