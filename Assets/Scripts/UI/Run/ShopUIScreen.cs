using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopUIScreen : UIScreen
{
    [SerializeField] private Transform slotContainer;
    [SerializeField] private ShopItemCardUI slotPrefab;
    [SerializeField] private TextMeshProUGUI currencyText;

    private ShopStation shop;
    private List<ShopItemCardUI> slots;

    protected override void OnBeforeOpen(object payload)
    {
        shop = (ShopStation)payload;

        foreach (Transform child in slotContainer)
            Destroy(child);
        slots.Clear();

        for (int i = 0; i < shop.Stock.Count; i++)
        {
            var slot = Instantiate(slotPrefab, slotContainer);
            slot.Setup(shop.Stock[i], () => HandleBuy(i));
            slots.Add(slot);
        }

        // setup currency tracker?
    }

    private void HandleBuy(int index)
    {
        if (shop.TryPurchase(index))
        {
            slots[index].MarkPurchased();
        }
        else
        {
            slots[index].BuyFailure();
        }
    }
}
