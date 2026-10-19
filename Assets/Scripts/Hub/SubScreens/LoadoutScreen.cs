using UnityEngine;

// Pick the weapon, protocol (run modifier) and tool for the next delve from the profile's unlocked gear.
// Any container left empty hides that section (tools are hidden in the demo).
public class LoadoutScreen : ListDetailScreen
{
    [Header("Loadout")]
    [SerializeField] private Transform weaponContainer;
    [SerializeField] private Transform modifierContainer;
    [SerializeField] private Transform toolContainer;

    [SerializeField] private string equippedTag = "[EQUIPPED]";
    [SerializeField] private string noWeaponLabel = "NONE";
    [SerializeField] [TextArea] private string noWeaponDescription = "Deploy unarmed. Melee only until a weapon is recovered.";
    [SerializeField] private string noModifierLabel = "NONE";
    [SerializeField] [TextArea] private string noModifierDescription = "Deploy without a protocol.";
    [SerializeField] private string noToolLabel = "NONE";
    [SerializeField] [TextArea] private string noToolDescription = "Deploy without an auxiliary tool.";

    private static readonly object NoWeaponMarker = new();
    private static readonly object NoModifierMarker = new();
    private static readonly object NoToolMarker = new();

    protected override void Rebuild()
    {
        ClearItems();

        var hub = HubManager.Instance;
        var meta = GameManager.Instance.Meta;
        var db = GameManager.Database;

        if (weaponContainer != null)
        {
            AddItem(weaponContainer).Setup(
                noWeaponLabel,
                onClick: () => { hub.SetLoadoutWeapon(null); RefreshTags(); },
                onHover: () => { if (detail != null) detail.Show(noWeaponLabel, noWeaponDescription); },
                data: NoWeaponMarker);

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
        }

        if (modifierContainer != null)
        {
            AddItem(modifierContainer).Setup(
                noModifierLabel,
                onClick: () => { hub.SetLoadoutModifier(null); RefreshTags(); },
                onHover: () => { if (detail != null) detail.Show(noModifierLabel, noModifierDescription); },
                data: NoModifierMarker);

            if (db.runModifiers != null)
            {
                foreach (var modifier in db.runModifiers.All)
                {
                    if (modifier == null || !meta.IsUnlocked(modifier)) continue;

                    var m = modifier;
                    AddItem(modifierContainer).Setup(
                        m.Name.ToUpperInvariant(),
                        onClick: () => { hub.SetLoadoutModifier(m); RefreshTags(); },
                        onHover: () => ShowGear(m),
                        data: m);
                }
            }
        }

        if (toolContainer != null)
        {
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
                RunModifier m => loadout.modifier == m,
                PlayerTool t => loadout.tool == t,
                _ => (item.Data == NoWeaponMarker && loadout.weapon == null)
                  || (item.Data == NoModifierMarker && loadout.modifier == null)
                  || (item.Data == NoToolMarker && loadout.tool == null)
            };

            item.SetTag(equipped ? equippedTag : null, highlight: true);
        }

        string weaponName = loadout.weapon != null ? loadout.weapon.Name.ToUpperInvariant() : noWeaponLabel;
        string modifierName = loadout.modifier != null ? loadout.modifier.Name.ToUpperInvariant() : noModifierLabel;
        SetHeaderInfo($"{weaponName} // {modifierName}");
    }

    private void ShowGear(ILootEntry entry)
    {
        if (detail == null) return;
        detail.Show(entry.Name.ToUpperInvariant(), entry.EffectDescription, entry.FlavourDescription);
    }
}
