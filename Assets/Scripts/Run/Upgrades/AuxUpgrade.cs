using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/NewAux")]
public class AuxUpgrade : UpgradeBase
{
    public AuxUpgrade()
    {
        slot = UpgradeSlot.None;
    }

    public virtual void OnStack() 
    {
        // default to apply again
    }

    public virtual void RemoveStack() { }
}
