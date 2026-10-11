using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopStation : StationBase, IInteractable
{    
    private List<LootResult> stock;
    private bool[] purchased;

    public IReadOnlyList<LootResult> Stock => stock;
    public IReadOnlyList<bool> Purchased => purchased;

    [Header("Config")]
    [SerializeField] private RewardCategory[] offerableTypes;

    protected override void OnSetup(MapNode node, RunState run)
    {
        stock = services.loot.RollAll(node, run);

        purchased = new bool[stock.Count];
    }

    protected override void OnInteract()
    {
        UIManager.Instance.OpenScreen("shopScreen", this);
    }

    public bool TryPurchase(int index)
    {
        if (purchased[index]) return false;

        var chosen = stock[index];

        if (run.TryUseCurrency(GetPrice(chosen.Item)))
        {
            run.GrantLoot(chosen);
            purchased[index] = true;

            return true;
        }
        return false;
    }

    private int GetPrice(ILootEntry loot)
    {
        // apply any run modifiers for price here
        return loot.ShopPrice;
    }
}
