using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/NewAux")]
public class AuxUpgrade : UpgradeBase
{
    protected int currentStacks;

    public AuxUpgrade()
    {
        slot = UpgradeSlot.None;
    }

    public override void Apply(Player player)
    {
        base.Apply(player);
        currentStacks = 1;
    }

    public override void Remove(Player player)
    {
        base.Remove(player);
        currentStacks = 0;
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
