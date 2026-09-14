using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionCardUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI flavourBox;
    [SerializeField] private TextMeshProUGUI effectBox;
    [SerializeField] private Button button;

    public void Setup(LootResult option, Action onPicked)
    {
        icon.sprite = option.Item.Icon;
        title.text = option.Item.Name;

        flavourBox.text = option.Item.FlavourDescription;
        effectBox.text = option.Item.EffectDescription;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onPicked?.Invoke());
    }

    public void ResetForPool()
    {
        if (icon != null)
            icon.sprite = null;

        if (title != null)
            title.text = string.Empty;

        if (flavourBox != null)
            flavourBox.text = string.Empty;

        if (effectBox != null)
            effectBox.text = string.Empty;

        button.onClick.RemoveAllListeners();
    }
}
