using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public struct LootResult<TEntry>
{
    public TEntry Item;
    public RarityTier Rarity;
}

public enum RarityTier
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public class LootManager : MonoBehaviour
{
    [SerializeField] private GameDatabase db;

    #region Generic rollers

    public List<TEntry> GetEligableCandidates<TEntry>(
        IEnumerable<TEntry> allItems, 
        Func<TEntry, bool> extraFiller = null) 
        where TEntry : ScriptableObject, IDatabaseEntry, IWeightedLoot
    {
        IEnumerable<TEntry> filtered = allItems;
        if(extraFiller != null)
            filtered = filtered.Where(extraFiller);

        return filtered.OrderBy(item => item.Id, StringComparer.Ordinal).ToList();
    }

    public float ComputeWeight<TEntry>(
        TEntry item, 
        RewardContext ctx, 
        RewardCategory category) 
        where TEntry : IDatabaseEntry, IWeightedLoot
    {
        float weight = item.BaseDropWeight;

        int pity = ctx.GetPity(category);
        weight *= 1f + (pity * 0.15f); // tune

        if (item.SynergyTags.Any(tag => ctx.ActiveSynergyTags.Contains(tag)))
            weight *= 1.5f;

        if (!ctx.SeenItemIdsThisRun.Contains(item.Id))
            weight *= 1.2f;

        return weight;
    }

    public List<TEntry> SampleIdentities<TEntry>(List<TEntry> candidates, Dictionary<TEntry, float> weights, int count)
    {
        var pool = new List<TEntry>(candidates);
        var chosen = new List<TEntry>();

        count = Mathf.Min(count, pool.Count);

        for (int i = 0; i < count; i++)
        {
            float totalWeight = pool.Sum(item => weights[item]);
            if (totalWeight <= 0f)
            {
                Debug.LogError("[RewardManager] No Weight setup");
                return null;
            }

            double roll = RNGManager.Instance.rng.NextDouble() * totalWeight;
            float cumulative = 0f;
            int pickedIndex = pool.Count - 1;

            for (int j = 0; j < pool.Count; j++)
            {
                cumulative += weights[pool[j]];
                if (roll < cumulative)
                {
                    pickedIndex = j;
                    break;
                }
            }

            chosen.Add(pool[pickedIndex]);
            pool.RemoveAt(pickedIndex);
        }

        return chosen;
    }

    public RarityTier RollRarity(int runDepth)
    {
        // replace with scriptable values.
        var weights = new Dictionary<RarityTier, float>
        {
            { RarityTier.Common,    60f },
            { RarityTier.Uncommon,  25f - runDepth * 0.5f },
            { RarityTier.Rare,      12f + runDepth * 0.3f },
            { RarityTier.Legendary,  3f + runDepth * 0.2f },
        };

        float total = weights.Values.Sum(w => MathF.Max(w, 0));
        double roll = RNGManager.Instance.rng.NextDouble();

        float cumulative = 0f;

        foreach (var kvp in weights)
        {
            cumulative += MathF.Max(kvp.Value, 0f);
            if (roll < cumulative) return kvp.Key;
        }

        return RarityTier.Common;
    }

    public List<LootResult<TEntry>> GenerateOffer<TEntry>(
        IEnumerable<TEntry> allItems, 
        RewardCategory category, 
        int offerCount, 
        RewardContext context, 
        int runDepth, 
        Func<TEntry, bool> extraFiller = null)
        where TEntry : ScriptableObject, IDatabaseEntry, IWeightedLoot
    {
        var rng = RNGManager.Instance.rng;

        var candidates = GetEligableCandidates(allItems, extraFiller);
        var weights = candidates.ToDictionary(item => item, item => ComputeWeight(item, context, category));
        var chosen = SampleIdentities(candidates, weights, offerCount);

        var offer = chosen
            .Select(item => new LootResult<TEntry> { Item = item, Rarity = RollRarity(runDepth) })
            .ToList();

        context.ResetPity(category);
        context.IncrementPityExcept(category);
        foreach (var item in chosen) context.SeenItemIdsThisRun.Add(item.Id);

        return offer;
    }

    #endregion


    public List<LootResult<UpgradeBase>> GenerateMajorUpgradeOffer(int offerCount, RewardContext ctx, int runDepth)
    {
        var pool = System.Enum.GetValues(typeof(UpgradeSlot))
            .Cast<UpgradeSlot>()
            .Where(slot => slot != UpgradeSlot.None && !ctx.FilledMajorSlots.Contains(slot))
            .SelectMany(slot => db.upgrades.GetByCategory(slot));

        return GenerateOffer(pool, RewardCategory.MajorUpgrade, offerCount, ctx, runDepth);
    }

    public List<LootResult<UpgradeBase>> GenerateAuxUpgradeOffer(int offerCount, RewardContext ctx, int runDepth)
    {
        var pool = db.upgrades.GetByCategory(UpgradeSlot.None);

        return GenerateOffer(pool, RewardCategory.AuxUpgrade, offerCount, ctx, runDepth);
    }
}
