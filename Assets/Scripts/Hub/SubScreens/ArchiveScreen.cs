using System.Collections.Generic;
using UnityEngine;

public struct ArchiveScreenPayload
{
    public IReadOnlyList<ArchiveData> AllEntries;
    public HashSet<string> UnlockedIDs;
}

public class ArchiveScreen : UIScreen
{
    [SerializeField] private Transform container;
    [SerializeField] private ArchiveEntryUI entryPrefab;

    private readonly List<ArchiveEntryUI> spawned = new();

    protected override void OnBeforeOpen(object payload)
    {
        var data = (ArchiveScreenPayload)payload;

        foreach (var instance in spawned)
            Destroy(instance.gameObject);

        spawned.Clear();

        foreach (var entry in data.AllEntries)
        {
            bool unlocked = data.UnlockedIDs.Contains(entry.Id);

            var tile = Instantiate(entryPrefab, container);
            tile.Setup(entry, unlocked);
            spawned.Add(tile);
        }
    }
}
