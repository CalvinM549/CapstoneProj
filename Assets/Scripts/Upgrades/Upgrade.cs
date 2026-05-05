using UnityEngine;

public enum UpgradeSlot
{
    Frame,
    Weapons,
    Brain,
    None
}

public class Upgrade : ScriptableObject
{
    [Header("Display")]
    public string upgradeName;
    [TextArea] public string flavourText;
    [TextArea] public string effectText;

    public Sprite icon;
    // Rarity?

    public UpgradeSlot slot;

    public int maxStacks;

    // Upgrade effects

    public void ApplyUpgrade() { }
    public void RemoveUpgrade() { }
}
