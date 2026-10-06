using System;
using UnityEngine;

public struct ResolvedOffer
{
    public RewardCategory category;
    public int count;
}

[Serializable]
public struct WeightedCategory
{
    public RewardCategory category;
    public float weight;
    public float weightPerDepth;
}

[Serializable]
public class OfferSlot
{
    public WeightedCategory[] categories;
    public int count = 3;
}

[Serializable]
public class RewardTable
{
    public OfferSlot[] slots = Array.Empty<OfferSlot>();
    public int fallbackCurrency = 25;

    public ResolvedOffer[] Resolve(int depth, System.Random rng)
    {
        var result = new ResolvedOffer[slots.Length];
        for (int currentSlot = 0; currentSlot < slots.Length; currentSlot++)
        {
            var validCategories = slots[currentSlot].categories;
            float total = 0f;

            foreach (var c in validCategories)
                total += Mathf.Max(0f, c.weight + c.weightPerDepth * depth);

            float roll = (float)rng.NextDouble() * total;
            var picked = validCategories[^1].category;
            foreach (var c in validCategories)
            {
                roll -= Mathf.Max(0f, c.weight + c.weightPerDepth * depth);
                if (roll < 0f)
                {
                    picked = c.category;
                    break;
                }
            }

            result[currentSlot] = new ResolvedOffer()
            {
                category = picked,
                count = slots[currentSlot].count
            };
        }

        return result;
    }
}
