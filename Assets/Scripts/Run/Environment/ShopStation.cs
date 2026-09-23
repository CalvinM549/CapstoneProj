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
        stock = RunManager.Instance.lootService.Roll(new LootRequest
            {
                category = node.rewards.category,
                count = 6,
                isShop = true,
                affectPity = true
            },
            run.rewardContext,
            GameRng.NodeRng(run.seed, node.coordinates),
            run.currentDepth);

        purchased = new bool[stock.Count];
    }

    protected override void OnInteract()
    {
        UIManager.Instance.OpenScreen("shopScreen", this);
    }

    public bool TryPurchase(int index)
    {
        if (purchased[index]) return false;

        if (run.TryUseCurrency(GetPrice(stock[index].Item)))
        {
            // Grant item
            purchased[index] = true;

            return true;
        }
        else
        {
            return false;
        }
    }

    private int GetPrice(ILootEntry loot)
    {
        return loot.ShopPrice;
    }
}
