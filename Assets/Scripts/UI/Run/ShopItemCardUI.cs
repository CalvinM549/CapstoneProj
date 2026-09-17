using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemCardUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI priceDisplay;

    [SerializeField] private Button button;

    public void Setup(LootResult option, Action onPicked)
    {
        icon.sprite = option.Item.Icon;
        priceDisplay.text = option.Item.ShopPrice.ToString(); // apply discounts?

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onPicked?.Invoke());
    }

    public void MarkPurchased()
    {
        //
    }

    public void BuyFailure()
    {
        //
    }


}
