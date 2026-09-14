using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Databases/UpgradeDatabase")]
public class UpgradeDatabase : CategorizedContentDatabase<UpgradeBase, UpgradeSlot>
{

    [ContextMenu("Auto-Populate All Items")]
    private void AutoFill()
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(UpgradeBase).Name}");
        entries = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<UpgradeBase>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(e => e != null)
            .ToArray();

        EditorUtility.SetDirty(this);
        Debug.Log($"[{name}] populated {entries.Length} entries");
    }

}
