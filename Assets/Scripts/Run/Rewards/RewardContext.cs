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
    public HashSet<UpgradeSlot> FilledMajorSlots = new();
    public Dictionary<string, int> OwnedUpgradeStacks = new();
    public HashSet<string> ownedWeaponIds = new();
    public HashSet<string> ownedToolIds = new();
    public HashSet<string> SeenItemIdsThisRun = new();
    public HashSet<string> ActiveSynergyTags = new();

    public Dictionary<RewardCategory, int> PityCounters = new();

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

    public int GetUpgradeStacks(string upgradeId) => OwnedUpgradeStacks.TryGetValue(upgradeId, out var stacks) ? stacks : 0;
}
