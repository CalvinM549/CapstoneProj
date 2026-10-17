using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct LootRequest
{
    public RewardCategory category;
    public int count;
    public bool isShop;
    public bool affectPity;
}

public struct LootResult
{
    public ILootEntry Item;
    public RarityTier Rarity;
}
public enum RarityTier
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

public class LootService
{
    // Weight tuning
    private const float PityWeightPerStep = 0.15f;
    private const float SynergyWeightPerPoint = 0.25f;  // per point of SynergyScore
    private const float MaxSynergyBonus = 1f;           // synergy can at most double an item's weight
    private const float UnseenBonus = 1.2f;
    private const int DominantTagMinCount = 2;          // build must own this many of a tag before variety kicks in

    private GameDatabase db;

    private List<ILootEntry> lootPool = new();
    private List<float> weights = new();

    public LootService(GameDatabase db)
    {
        this.db = db;
    }

    // rerollIndex 0 is the original offer. Each reroll gets its own seeded stream, so reroll N at a node
    // is identical for the same seed. 'exclude' keeps the cards currently on screen out of the new roll where possible.
    public List<LootResult> RollSlot(MapNode node, RunState run, int slot, int rerollIndex = 0, ICollection<string> exclude = null)
    {
        if(slot < 0 || slot >= node.offers.Length) return new List<LootResult>();

        var offer = node.offers[slot];
        int salt = rerollIndex == 0 ? slot + 1 : (slot + 1) + rerollIndex * 7919;
        var rng = RNGManager.NodeRng(run.map.seed, node.coordinates, salt);

        var request = new LootRequest
        {
            category = offer.category,
            count = offer.count,
            isShop = node.type.isShop
        };

        var result = Roll(request, run.rewardContext, rng, node.depth, exclude);

        // Not enough fresh items left: keep the fresh ones, then fill the gap with repeats
        if (exclude != null && exclude.Count > 0 && result.Count < offer.count)
        {
            var taken = new HashSet<string>();
            foreach (var r in result) taken.Add(r.Item.Id);

            var fill = request;
            fill.count = offer.count - result.Count;
            result.AddRange(Roll(fill, run.rewardContext, rng, node.depth, taken));
        }

        return result;
    }

    public List<LootResult> RollAll(MapNode node, RunState run)
    {
        var all = new List<LootResult>();
        for (int i = 0; i < node.offers.Length; i++)
        {
            all.AddRange(RollSlot(node, run, i));
        }
        return all;
    }

    public List<LootResult> Roll(LootRequest request, RewardContext ctx, System.Random rng, int depth, ICollection<string> exclude = null)
    {
        BuildPool(request, ctx);

        if (exclude != null && exclude.Count > 0)
        {
            for (int i = lootPool.Count - 1; i >= 0; i--)
            {
                if (!exclude.Contains(lootPool[i].Id)) continue;
                lootPool.RemoveAt(i);
                weights.RemoveAt(i);
            }
        }

        var result = new List<LootResult>(request.count);

        // Variety guarantee: in offers of 3+, at most count-1 cards may share the build's dominant tag,
        // so there is always at least one option pulling somewhere else.
        string dominant = ctx.DominantTag(DominantTagMinCount);
        int maxDominant = request.count >= 3 ? request.count - 1 : request.count;
        int dominantPicked = 0;

        for (int i = 0; i < request.count && lootPool.Count > 0; i++)
        {
            if (dominant != null && dominantPicked >= maxDominant)
                SuppressTag(dominant);

            float total = 0f;
            for (int j = 0; j < weights.Count; j++)
                total += weights[j];

            if (total <= 0f) break;

            float roll = (float)rng.NextDouble() * total;
            int pick = lootPool.Count - 1;
            for (int j = 0; j < weights.Count; j++)
            {
                roll -= weights[j];
                if (roll < 0f)
                {
                    pick = j;
                    break;
                }
            }

            result.Add(new LootResult
            {
                Item = lootPool[pick],
                Rarity = RollRarity(depth, rng)
            });

            if (dominant != null && HasTag(lootPool[pick], dominant)) dominantPicked++;

            lootPool.RemoveAt(pick);
            weights.RemoveAt(pick);

        }

        if (request.affectPity) ctx.RegisterOffer(request.category, result);
        return result;

    }

    private void BuildPool(LootRequest request, RewardContext ctx)
    {
        lootPool.Clear();
        weights.Clear();

        switch (request.category)
        {
            case RewardCategory.MajorUpgrade:
                foreach (var u in db.upgrades.All)
                {
                    if (u.slot != UpgradeSlot.None && !ctx.IsMajorSlotFilled(u.slot))
                        TryAdd(u, request, ctx);
                }
                break;

            case RewardCategory.AuxUpgrade:
                foreach (var u in db.upgrades.GetByCategory(UpgradeSlot.None))
                {
                    TryAdd(u, request, ctx);
                }
                break;

            case RewardCategory.Weapon:
                foreach(var w in db.weapons.All) TryAdd(w, request, ctx);
                break;

            case RewardCategory.Tool:
                foreach(var t in db.tools.All) TryAdd(t, request, ctx);
                break;
        }
    }

    private void TryAdd(ILootEntry item, LootRequest request, RewardContext ctx)
    {
        if (request.isShop && !item.Buyable) return;

        if (ctx.IsOwnedAtMax(item)) return;
        lootPool.Add(item);
        weights.Add(ComputeWeight(item, ctx, request.category));
    }

    private float ComputeWeight(ILootEntry item, RewardContext ctx, RewardCategory category)
    {
        float weight = item.BaseDropWeight;

        int pity = ctx.GetPity(category);
        weight *= 1f + (pity * PityWeightPerStep);

        // Soft pull towards the build: scales with how invested the build is in this item's tags
        int synergy = ctx.SynergyScore(item);
        if (synergy > 0)
            weight *= 1f + Mathf.Min(synergy * SynergyWeightPerPoint, MaxSynergyBonus);

        if (!ctx.SeenItemIdsThisRun.Contains(item.Id))
            weight *= UnseenBonus;

        return weight;
    }

    private static bool HasTag(ILootEntry item, string tag)
    {
        foreach (var t in item.SynergyTags)
            if (t == tag) return true;
        return false;
    }

    // Zero the weight of items carrying 'tag', unless that would leave nothing to pick
    private void SuppressTag(string tag)
    {
        bool anyOther = false;
        for (int j = 0; j < lootPool.Count; j++)
            if (weights[j] > 0f && !HasTag(lootPool[j], tag)) { anyOther = true; break; }

        if (!anyOther) return;

        for (int j = 0; j < lootPool.Count; j++)
            if (HasTag(lootPool[j], tag)) weights[j] = 0f;
    }

    private RarityTier RollRarity(int runDepth, System.Random rng)
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
        double roll = rng.NextDouble() * total;

        float cumulative = 0f;

        foreach (var kvp in weights)
        {
            cumulative += MathF.Max(kvp.Value, 0f);
            if (roll < cumulative) return kvp.Key;
        }

        return RarityTier.Common;
    }
}
