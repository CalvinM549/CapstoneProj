using UnityEngine;

public enum UpgradeSlot
{
    Frame,
    Weapons,
    Brain,
    None
}

public abstract class Upgrade : ScriptableObject
{
    public UpgradeSlot slot;
    public string upgradeName;
    [TextArea] public string description;

    public abstract void ApplyUpgrade();
    public abstract void RemoveUpgrade();
}
