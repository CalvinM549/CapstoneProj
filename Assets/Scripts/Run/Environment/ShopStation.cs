using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopStation : MonoBehaviour, IInteractable
{    
    private RunState run;
    private List<LootResult> stock;
    private bool[] purchased;

    public IReadOnlyList<LootResult> Stock => stock;
    public IReadOnlyList<bool> Purchased => purchased;

    [Header("Config")]
    [SerializeField] private RewardCategory[] offerableTypes;

    public string InteractPrompt => "Open wares";

    public void Setup(RoomData room, RunState run)
    {
        this.run = run;

        stock = offerableTypes
            .SelectMany(type => LootManager.Instance.GenerateLootOffer(1, type, run.rewardContext, run.currentDepth))
            .ToList();

        purchased = new bool[stock.Count];
    }

    public void Interact()
    {
        // open UI screen
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

    private int GetPrice(IRollableLoot loot)
    {
        return loot.ShopPrice;
    }
}
