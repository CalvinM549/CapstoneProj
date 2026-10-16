using UnityEngine;

// Pick the weapon + tool for the next delve from the profile's unlocked gear.
public class LoadoutScreen : ListDetailScreen
{
    [Header("Loadout")]
    [SerializeField] private Transform weaponContainer;
    [SerializeField] private Transform toolContainer;

    [SerializeField] private string equippedTag = "[EQUIPPED]";
    [SerializeField] private string noToolLabel = "NONE";
    [SerializeField] [TextArea] private string noToolDescription = "Deploy without an auxiliary tool.";

    private static readonly object NoToolMarker = new();

    protected override void Rebuild()
    {
        ClearItems();

        var hub = HubManager.Instance;
        var meta = GameManager.Instance.Meta;
        var db = GameManager.Database;

        foreach (var weapon in db.weapons.All)
        {
            if (weapon == null || !meta.IsUnlocked(weapon)) continue;

            var w = weapon;
            AddItem(weaponContainer).Setup(
                w.Name.ToUpperInvariant(),
                onClick: () => { hub.SetLoadoutWeapon(w); RefreshTags(); },
                onHover: () => ShowGear(w),
                data: w);
        }

        AddItem(toolContainer).Setup(
            noToolLabel,
            onClick: () => { hub.SetLoadoutTool(null); RefreshTags(); },
            onHover: () => { if (detail != null) detail.Show(noToolLabel, noToolDescription); },
            data: NoToolMarker);

        foreach (var tool in db.tools.All)
        {
            if (tool == null || !meta.IsUnlocked(tool)) continue;

            var t = tool;
            AddItem(toolContainer).Setup(
                t.Name.ToUpperInvariant(),
                onClick: () => { hub.SetLoadoutTool(t); RefreshTags(); },
                onHover: () => ShowGear(t),
                data: t);
        }

        RefreshTags();
    }

    private void RefreshTags()
    {
        var loadout = HubManager.Instance.PendingLoadout;

        foreach (var item in items)
        {
            bool equipped = item.Data switch
            {
                PlayerWeapon w => loadout.weapon == w,
                PlayerTool t => loadout.tool == t,
                _ => item.Data == NoToolMarker && loadout.tool == null
            };

            item.SetTag(equipped ? equippedTag : null, highlight: true);
        }

        string weaponName = loadout.weapon != null ? loadout.weapon.Name.ToUpperInvariant() : "--";
        string toolName = loadout.tool != null ? loadout.tool.Name.ToUpperInvariant() : noToolLabel;
        SetHeaderInfo($"{weaponName} // {toolName}");
    }

    private void ShowGear(ILootEntry entry)
    {
        if (detail == null) return;
        detail.Show(entry.Name.ToUpperInvariant(), entry.EffectDescription, entry.FlavourDescription);
    }
}
