using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/NewAux")]
public class AuxUpgrade : UpgradeBase
{
    protected int currentStacks;

    public AuxUpgrade()
    {
        slot = UpgradeSlot.None;
    }

    public virtual void OnStack() 
    {
        currentStacks++;
        ApplyStatModifiers(player.Stats);
    }

    public virtual void RemoveStack()
    {
        currentStacks = Mathf.Max(1, currentStacks - 1);
    }
}
