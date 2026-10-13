using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionCardUI : MonoBehaviour
{
    public CanvasGroup cg;

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI flavourBox;
    [SerializeField] private TextMeshProUGUI effectBox;


    [SerializeField] private TextMeshProUGUI slotTag;
    [SerializeField] private TextMeshProUGUI noteBox;
    [SerializeField] private Button button;

    public void Setup(LootResult option, Action onPicked)
    {
        Player p = RunManager.CurrentRun.player;

        icon.sprite = option.Item.Icon;
        title.text = option.Item.Name;

        flavourBox.text = option.Item.FlavourDescription;
        effectBox.text = option.Item.EffectDescription;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onPicked?.Invoke());

        noteBox.text = string.Empty;

        switch (option.Item)
        {
            case MajorUpgrade major:
                slotTag.text = major.slot.ToString().ToUpper();
                var current = p.Upgrades.GetMajor(major.slot);
                if (current != null && !current.isTemplate)
                    noteBox.text = $"REPLACES {current.upgradeName}";

                break;

            case AuxUpgrade aux:
                slotTag.text = "AUX";
                noteBox.text = $"[{p.Upgrades.GetAuxStacks(aux)} / {aux.maxStacks}]";
                break;

            case PlayerWeapon weapon:
                slotTag.text = "WEAPON";
                break;

            case PlayerTool tool:
                slotTag.text = "TOOL";
                break;
        }
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
