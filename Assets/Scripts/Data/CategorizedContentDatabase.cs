using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICategorizedEntry<TCategory> : IDatabaseEntry
{
    TCategory Category { get; }
}

public class CategorizedContentDatabase<TEntry, TCategory> : ContentDatabase<TEntry> where TEntry : ScriptableObject, ICategorizedEntry<TCategory>
{
    private Dictionary<TCategory, List<TEntry>> byCategory;

    protected override void OnEnable()
    {
        base.OnEnable();
        byCategory = null;
    }

    private void BuildCategoryLookupIfNeeded()
    {
        if (byCategory == null) return;

        byCategory = new Dictionary<TCategory, List<TEntry>>();
        foreach (var entry in All)
        {
            if(entry == null) continue;

            if (!byCategory.TryGetValue(entry.Category, out var list))
            {
                list = new List<TEntry>();
                byCategory[entry.Category] = list;
            }

            list.Add(entry);
        }
    }

    public IReadOnlyList<TEntry> GetByCategory(TCategory category)
    {
        BuildCategoryLookupIfNeeded();
        return byCategory.TryGetValue(category, out var list) ? list : Array.Empty<TEntry>();
    }
}
