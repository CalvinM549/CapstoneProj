using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/NewAux")]
public class AuxUpgrade : UpgradeBase
{
    public AuxUpgrade()
    {
        slot = UpgradeSlot.None;
    }
}
