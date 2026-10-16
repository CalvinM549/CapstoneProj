using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Reward choice card. Terminal-styled: rarity-tinted frame + header, hover cursor on the title.
public class OptionCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public CanvasGroup cg;

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI flavourBox;
    [SerializeField] private TextMeshProUGUI effectBox;

    [SerializeField] private TextMeshProUGUI slotTag;
    [SerializeField] private TextMeshProUGUI noteBox;
    [SerializeField] private Button button;

    [Header("Rarity / Accent (optional)")]
    [SerializeField] private TextMeshProUGUI rarityText;
    [Tooltip("Tinted with the rarity colour (frame lines, header bar, etc).")]
    [SerializeField] private Graphic[] accentGraphics = Array.Empty<Graphic>();
    [Tooltip("Index matches RarityTier: Common, Uncommon, Rare, Legendary")]
    [SerializeField] private Color[] rarityColours =
    {
        new(0.75f, 0.75f, 0.75f),
        new(0.49f, 1f, 0.63f),
        new(0.5f, 0.85f, 1f),
        new(1f, 0.85f, 0.4f)
    };

    [Header("Hover")]
    [SerializeField] private float idleAccentAlpha = 0.55f;
    [SerializeField] private float hoverAccentAlpha = 1f;
    [SerializeField] private string hoverSfx = "UItick";

    private string baseTitle;
    private Color accent = Color.white;

    public void Setup(LootResult option, Action onPicked)
    {
        Player p = RunManager.CurrentRun.player;

        if (icon != null)
        {
            icon.sprite = option.Item.Icon;
            icon.enabled = option.Item.Icon != null;
        }

        baseTitle = option.Item.Name.ToUpperInvariant();
        title.text = baseTitle;

        flavourBox.text = option.Item.FlavourDescription;
        effectBox.text = option.Item.EffectDescription;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onPicked?.Invoke());

        noteBox.text = string.Empty;

        switch (option.Item)
        {
            case MajorUpgrade major:
                slotTag.text = $"MAJOR // {major.slot.ToString().ToUpperInvariant()}";
                var current = p.Upgrades.GetMajor(major.slot);
                if (current != null && !current.isTemplate)
                    noteBox.text = $"REPLACES {current.upgradeName.ToUpperInvariant()}";

                break;

            case AuxUpgrade aux:
                slotTag.text = "AUX";
                noteBox.text = $"STACKS [{p.Upgrades.GetAuxStacks(aux)}/{aux.maxStacks}]";
                break;

            case PlayerWeapon weapon:
                slotTag.text = "WEAPON";
                if (p.Combat.HasWeaponEquipped)
                    noteBox.text = $"REPLACES {p.Combat.EquippedWeapon.data.Name.ToUpperInvariant()}";
                break;

            case PlayerTool tool:
                slotTag.text = "TOOL";
                if (p.Tools.HasToolEquipped)
                    noteBox.text = $"REPLACES {p.Tools.EquippedTool.data.Name.ToUpperInvariant()}";
                break;
        }

        int rarity = Mathf.Clamp((int)option.Rarity, 0, rarityColours.Length - 1);
        accent = rarityColours.Length > 0 ? rarityColours[rarity] : Color.white;
        if (rarityText != null) rarityText.text = $"// {option.Rarity.ToString().ToUpperInvariant()}";

        SetHover(false);
    }

    public void SetInteractable(bool value) => button.interactable = value;

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

    #region Hover

    public void OnPointerEnter(PointerEventData eventData) => SetHover(true, true);
    public void OnPointerExit(PointerEventData eventData) => SetHover(false);
    public void OnSelect(BaseEventData eventData) => SetHover(true, true);
    public void OnDeselect(BaseEventData eventData) => SetHover(false);

    private void SetHover(bool hovered, bool playSound = false)
    {
        if (hovered && !button.interactable) hovered = false;

        title.text = hovered ? $"> {baseTitle}_" : baseTitle;

        Color c = accent;
        c.a = hovered ? hoverAccentAlpha : idleAccentAlpha;
        foreach (var g in accentGraphics)
            if (g != null) g.color = c;

        if (rarityText != null) rarityText.color = accent;

        if (playSound && hovered && AudioManager.Instance != null && !string.IsNullOrEmpty(hoverSfx))
            AudioManager.Instance.PlaySFX(hoverSfx, Vector3.zero);
    }

    #endregion
}
