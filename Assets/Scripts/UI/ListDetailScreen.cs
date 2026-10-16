using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Base for screens that are "a list of entries + a detail pane" (Loadout, Unlocks, Archive).
// Subclasses only implement Rebuild() and call AddItem(container) for each row.
public abstract class ListDetailScreen : UIScreen
{
    [Header("List / Detail")]
    [SerializeField] protected TerminalListItem itemPrefab;
    [SerializeField] protected TerminalDetailPanel detail;
    [Tooltip("Optional line in the header for screen-specific info (e.g. SHARDS 120).")]
    [SerializeField] protected TextMeshProUGUI headerInfo;

    protected readonly List<TerminalListItem> items = new();

    protected override void OnBeforeOpen(object payload)
    {
        if (detail != null) detail.Clear();
        Rebuild();
    }

    // Returning from a confirm box / child screen
    protected override void OnFocusRegained() => Rebuild();

    protected abstract void Rebuild();

    protected TerminalListItem AddItem(Transform container)
    {
        var item = Instantiate(itemPrefab, container);
        item.gameObject.SetActive(true);
        items.Add(item);
        return item;
    }

    protected void ClearItems()
    {
        foreach (var item in items)
        {
            if (item == null) continue;
            item.gameObject.SetActive(false); // drop out of layout immediately
            Destroy(item.gameObject);
        }

        items.Clear();
    }

    protected void SetHeaderInfo(string text)
    {
        if (headerInfo != null) headerInfo.text = text;
    }

    public void OnBackClicked() => UIManager.Instance.CloseTop();
}
