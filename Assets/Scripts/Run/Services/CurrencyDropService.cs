using System.Collections.Generic;
using UnityEngine;

public class CurrencyDropService
{
    private readonly CurrencyPickup prefab;
    private readonly Transform container;
    private readonly ObjectPool<CurrencyPickup> pool;

    private readonly List<CurrencyPickup> active = new();

    private const int initialPoolSize = 15;

    public CurrencyDropService(CurrencyPickup prefab, Transform container)
    {
        this.prefab = prefab;
        this.container = container;

        pool = new(prefab, initialPoolSize, container);
    }

    public void DropBurst(Vector2 pos, int totalValue, int count)
    {
        foreach (int piece in SplitValue(totalValue, count))
        {
            var pickup = pool.Get();
            active.Add(pickup);
            pickup.Spawn(pos, piece, () => 
            { 
                active.Remove(pickup);
                pool.ReturnToPool(pickup); 
            });
        }
    }

    public void ForceCollectAll()
    {
        foreach (var pickup in new List<CurrencyPickup>(active))
        {
            pickup.ForceCollect();
        }
    }

    private int[] SplitValue(int total, int pieces)
    {
        int[] result = new int[pieces];
        int baseAmt = total / pieces, remainder = total % pieces;
        for (int i = 0; i < pieces; i++)
        {
            result[i] = baseAmt + (i < remainder ? 1 : 0);
        }

        return result;
    }
}
