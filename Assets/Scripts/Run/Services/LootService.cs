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
    private GameDatabase db;

    private List<ILootEntry> lootPool = new();
    private List<float> weights = new();

    public LootService(GameDatabase db)
    {
        this.db = db;
    }

    public List<LootResult> Roll(LootRequest request, RewardContext ctx, System.Random rng, int depth)
    {
        BuildPool(request, ctx);

        var result = new List<LootResult>(request.count);

        for (int i = 0; i < request.count && lootPool.Count > 0; i++)
        {
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
        weight *= 1f + (pity * 0.15f); // tune

        if (item.SynergyTags.Any(tag => ctx.ActiveSynergyTags.Contains(tag)))
            weight *= 1.5f;

        if (!ctx.SeenItemIdsThisRun.Contains(item.Id))
            weight *= 1.2f;

        return weight;
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
