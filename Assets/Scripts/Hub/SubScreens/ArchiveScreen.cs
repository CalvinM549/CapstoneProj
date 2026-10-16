using System.Collections.Generic;
using UnityEngine;

public struct ArchiveScreenPayload
{
    public IReadOnlyList<ArchiveData> AllEntries;
    public List<string> UnlockedIDs;
}

// Recovered lore logs. Locked entries are listed but unreadable.
public class ArchiveScreen : ListDetailScreen
{
    [Header("Archive")]
    [SerializeField] private Transform container;

    [SerializeField] private string lockedLabelFormat = "??? // LOG {0:00}";
    [SerializeField] private string lockedTitle = "[DATA CORRUPTED]";
    [SerializeField] [TextArea] private string lockedBody = "> SIGNAL FRAGMENT UNRECOVERABLE\n> RECOVER THIS LOG IN THE FIELD";
    [SerializeField] private string emptyLabel = "NO LOGS INDEXED";

    private ArchiveScreenPayload data;

    protected override void OnBeforeOpen(object payload)
    {
        data = payload is ArchiveScreenPayload p && p.AllEntries != null
            ? p
            : new ArchiveScreenPayload
            {
                AllEntries = GameManager.Database.archives.All,
                UnlockedIDs = GameManager.Instance.ActiveProfile?.unlockedArchives ?? new List<string>()
            };

        base.OnBeforeOpen(payload);
    }

    protected override void Rebuild()
    {
        ClearItems();

        int unlockedCount = 0;

        for (int i = 0; i < data.AllEntries.Count; i++)
        {
            var entry = data.AllEntries[i];
            if (entry == null) continue;

            bool unlocked = data.UnlockedIDs.Contains(entry.Id);
            if (unlocked) unlockedCount++;

            string label = unlocked ? entry.Title.ToUpperInvariant() : string.Format(lockedLabelFormat, i + 1);

            AddItem(container).Setup(
                label,
                onHover: () => ShowEntry(entry, unlocked),
                data: entry);
        }

        if (items.Count == 0)
            AddItem(container).Setup(emptyLabel, interactable: false);

        SetHeaderInfo($"LOGS RECOVERED {unlockedCount}/{data.AllEntries.Count}");
    }

    private void ShowEntry(ArchiveData entry, bool unlocked)
    {
        if (detail == null) return;

        if (unlocked)
            detail.Show(entry.Title.ToUpperInvariant(), entry.Text);
        else
            detail.Show(lockedTitle, lockedBody);
    }
}
