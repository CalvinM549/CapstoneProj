using UnityEngine;

// Spend shards (profile.metaCurrency) on permanent weapon / protocol / tool unlocks.
// Any container left empty hides that section (tools are hidden in the demo).
public class UnlocksScreen : ListDetailScreen
{
    [Header("Unlocks")]
    [SerializeField] private Transform weaponContainer;
    [SerializeField] private Transform modifierContainer;
    [SerializeField] private Transform toolContainer;

    [SerializeField] private string currencyName = "SHARDS";
    [SerializeField] private string currencyShort = "SH";
    [SerializeField] private string ownedTag = "[OWNED]";

    [Header("Feedback")]
    [SerializeField] private string purchaseSfx = "UItick";

    protected override void Rebuild()
    {
        ClearItems();

        var db = GameManager.Database;

        if (weaponContainer != null)
            foreach (var weapon in db.weapons.All)
                AddEntry(weaponContainer, weapon);

        if (modifierContainer != null && db.runModifiers != null)
            foreach (var modifier in db.runModifiers.All)
                AddEntry(modifierContainer, modifier);

        if (toolContainer != null)
            foreach (var tool in db.tools.All)
                AddEntry(toolContainer, tool);

        SetHeaderInfo($"{currencyName} {GameManager.Instance.Meta.Currency}");
    }

    private void AddEntry(Transform container, IUnlockable entry)
    {
        if (entry == null || !entry.ListInUnlocks || entry.UnlockedByDefault) return;

        var meta = GameManager.Instance.Meta;
        bool owned = meta.IsUnlocked(entry);
        bool affordable = meta.CanAfford(entry.UnlockCost);

        var item = AddItem(container).Setup(
            entry.Name.ToUpperInvariant(),
            onClick: () => HandleClicked(entry),
            onHover: () => ShowEntry(entry),
            data: entry);

        item.SetTag(owned ? ownedTag : $"{entry.UnlockCost} {currencyShort}", highlight: !owned && affordable);
    }

    private void ShowEntry(IUnlockable entry)
    {
        if (detail == null) return;

        var meta = GameManager.Instance.Meta;
        string footer = meta.IsUnlocked(entry)
            ? "> UNLOCKED // AVAILABLE IN LOADOUT"
            : meta.CanAfford(entry.UnlockCost)
                ? $"> {entry.UnlockCost} {currencyName} // CLICK TO UNLOCK"
                : $"> {entry.UnlockCost} {currencyName} // INSUFFICIENT";

        detail.Show(entry.Name.ToUpperInvariant(), entry.EffectDescription, footer);
    }

    private void HandleClicked(IUnlockable entry)
    {
        var meta = GameManager.Instance.Meta;
        if (meta.IsUnlocked(entry) || !meta.CanAfford(entry.UnlockCost))
        {
            ShowEntry(entry);
            return;
        }

        UIManager.Instance.OpenScreen("confirmScreen", new ConfirmRequest(
            $"UNLOCK {entry.Name.ToUpperInvariant()} FOR {entry.UnlockCost} {currencyName}?",
            () =>
            {
                if (meta.TryPurchase(entry) && AudioManager.Instance != null && !string.IsNullOrEmpty(purchaseSfx))
                    AudioManager.Instance.PlaySFX(purchaseSfx, Vector3.zero);

                Rebuild();
                ShowEntry(entry);
            },
            "UNLOCK"));
    }

#if UNITY_EDITOR
    [ContextMenu("Debug/Grant 100 Shards")]
    private void DebugGrantShards()
    {
        if (!Application.isPlaying || GameManager.Instance == null || GameManager.Instance.Meta == null) return;

        GameManager.Instance.Meta.GrantCurrency(100);
        GameManager.Instance.SaveActiveProfile();
        if (gameObject.activeInHierarchy) Rebuild();
    }
#endif
}
