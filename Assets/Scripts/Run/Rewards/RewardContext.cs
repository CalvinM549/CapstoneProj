using System.Collections.Generic;
using System.Linq;
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

    #region Creation / Loading

    public RewardContext(PlayerUpgrades upgrades)
    {
        this.upgrades = upgrades;
    }

    public RewardSaveData ToSave()
    {
        return new RewardSaveData()
        {
            ownedWeaponIds = ownedWeaponIds.ToList(),
            ownedToolIds = ownedToolIds.ToList(),
            seenItemIds = SeenItemIdsThisRun.ToList(),
        };
    }

    public void FromSave(RewardSaveData save)
    {
        ownedWeaponIds = new HashSet<string>(save.ownedWeaponIds);
        ownedToolIds = new HashSet<string>(save.ownedToolIds);

        SeenItemIdsThisRun = new HashSet<string>(save.seenItemIds);
    }

    #endregion


    public bool IsMajorSlotFilled(UpgradeSlot slot) => upgrades.GetMajor(slot) != null;
    public int GetAuxStacks(AuxUpgrade upgrade) => upgrades.GetAuxStacks(upgrade);

    public bool IsOwnedAtMax(ILootEntry entry)
    {
        switch (entry)
        {
            case AuxUpgrade upgrade:
                return upgrades.GetAuxStacks(upgrade) >= upgrade.maxStacks;

            case PlayerWeapon weapon:
                return ownedWeaponIds.Contains(weapon.Id);

            case PlayerTool tool:
                return ownedToolIds.Contains(tool.Id);

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
