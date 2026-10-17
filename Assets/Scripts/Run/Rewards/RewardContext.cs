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

    // Archetype tags the current build carries, and how strongly (one count per upgrade stack / equipped item)
    public HashSet<string> ActiveSynergyTags = new();
    public readonly Dictionary<string, int> SynergyTagCounts = new();

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

    #region Synergy

    // Rebuild tag counts from what the player currently has. Call after any loot is granted.
    public void RefreshSynergyTags(Player player)
    {
        SynergyTagCounts.Clear();

        void Add(IReadOnlyList<string> tags, int weight)
        {
            if (tags == null || weight <= 0) return;
            foreach (var tag in tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                SynergyTagCounts[tag] = SynergyTagCounts.GetValueOrDefault(tag) + weight;
            }
        }

        foreach (var upgrade in upgrades.AllActiveUpgrades)
            Add(upgrade.SynergyTags, upgrade is AuxUpgrade aux ? Mathf.Max(1, upgrades.GetAuxStacks(aux)) : 1);

        if (player != null)
        {
            if (player.Combat.HasWeaponEquipped) Add(player.Combat.EquippedWeapon.data.SynergyTags, 1);
            if (player.Tools.HasToolEquipped) Add(player.Tools.EquippedTool.data.SynergyTags, 1);
        }

        ActiveSynergyTags = new HashSet<string>(SynergyTagCounts.Keys);
    }

    // Sum of how invested the build is in each of the item's tags (each tag capped so one tag can't dominate)
    public int SynergyScore(ILootEntry item, int perTagCap = 3)
    {
        int score = 0;
        foreach (var tag in item.SynergyTags)
            score += Mathf.Min(SynergyTagCounts.GetValueOrDefault(tag), perTagCap);
        return score;
    }

    // The tag the build leans on most, or null if nothing has reached minCount yet
    public string DominantTag(int minCount = 2)
    {
        string best = null;
        int bestCount = minCount - 1;
        foreach (var pair in SynergyTagCounts)
        {
            if (pair.Value > bestCount)
            {
                best = pair.Key;
                bestCount = pair.Value;
            }
        }
        return best;
    }

    #endregion

    // Templates are placeholders, so they don't count as a filled slot
    public bool IsMajorSlotFilled(UpgradeSlot slot)
    {
        var major = upgrades.GetMajor(slot);
        return major != null && !major.isTemplate;
    }

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
            PityCounters[c] = GetPity(c) + 1;
        }
    }

    public void RegisterOffer(RewardCategory category, List<LootResult> result)
    {
        ResetPity(category);
        IncrementPityExcept(category);
    }
}
