using UnityEngine;

public abstract class UpgradeEffect : ScriptableObject
{
    public abstract void Apply(Player player);
    public abstract void Remove(Player player);
}
