using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopUIScreen : UIScreen
{
    [SerializeField] private Transform slotContainer;
    [SerializeField] private ShopItemCardUI slotPrefab;

    [SerializeField] private TextMeshProUGUI selectedTitle;
    [SerializeField] private TextMeshProUGUI selectedDescription;
    [SerializeField] private TextMeshProUGUI selectedFlavour;

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
    }

    private void UpdateSelected(int index)
    {
        var item = shop.Stock[index].Item;

        selectedTitle.text = item.Name;
        selectedDescription.text = item.EffectDescription;
        selectedFlavour.text = item.FlavourDescription;
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
