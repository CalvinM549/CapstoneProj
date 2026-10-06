using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Databases/RoomDatabase")]
public class RoomDatabase : CategorizedContentDatabase<RoomData, RoomTypeData>
{
    public IEnumerable<RoomData> GetCandidates(RoomTypeData type, int chapter)
    {
        foreach (var room in GetByCategory(type))
        {
            if (room == null) continue;

            if (chapter < room.minChapter || chapter > room.maxChapter) continue;

            yield return room;
        }
    }

#if UNITY_EDITOR

    [ContextMenu("Auto-Populate All Items")]
    private void AutoFill()
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(RoomData).Name}");
        entries = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<RoomData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(e => e != null)
            .ToArray();

        EditorUtility.SetDirty(this);
        Debug.Log($"[{name}] populated {entries.Length} entries");
    }

#endif
}