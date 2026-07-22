using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContentDatabase<TEntry> : ScriptableObject where TEntry : ScriptableObject, IDatabaseEntry
{
    [SerializeField] private TEntry[] entries = Array.Empty<TEntry>();

    private Dictionary<string, TEntry> lookup;

    public IReadOnlyList<TEntry> All => entries;

    protected virtual void OnEnable() => lookup = null;

    private void BuildLookupIfNeeded()
    {
        if (lookup != null) return;

        lookup = new Dictionary<string, TEntry>(entries.Length);

        foreach (var entry in entries)
        {
            if(entry == null) continue;

            if (string.IsNullOrEmpty(entry.Id))
            {
                Debug.LogError($"[{name}] entry '{entry.name}' has no ID");
                continue;
            }

            if (!lookup.TryAdd(entry.Id, entry))
                Debug.LogError($"[{name}] duplicate ID {entry.Id} is used");
        }
    }

    public bool TryGet(string id, out TEntry entry)
    {
        BuildLookupIfNeeded();
        return lookup.TryGetValue(id, out entry);
    }

    public TEntry Get(string id)
    {
        BuildLookupIfNeeded();
        return lookup.TryGetValue(id, out TEntry entry) ? entry : null;
    }

    public TEntry GetRandom(int seed)
    {
        BuildLookupIfNeeded();
        UnityEngine.Random.InitState(seed);
        int randomIndex = UnityEngine.Random.Range(0, All.Count);
        return All[randomIndex];
    }

    public bool Contains(string id)
    {
        BuildLookupIfNeeded();
        return lookup.ContainsKey(id);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        lookup = null;
        ValidateNoDuplicates();
    }

    private void ValidateNoDuplicates()
    {
        var seen = new HashSet<string>();
        foreach (var entry in entries)
        {
            if (entry == null) continue;
            if (string.IsNullOrEmpty(entry.Id))
                Debug.LogWarning($"[{name}] {entry.name} has no ID set");
            else if (!seen.Add(entry.Id))
                Debug.LogError($"[{name}] Duplicate ID {entry.Id} found");
        }
    }

    [ContextMenu("Auto-Populate From Project")]
    private void AutoPopulate()
    {
        var guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(TEntry).Name}");
        entries = guids
            .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<TEntry>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
            .Where(e => e != null)
            .ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[{name}] populated {entries.Length} entries");
    }

#endif
}
