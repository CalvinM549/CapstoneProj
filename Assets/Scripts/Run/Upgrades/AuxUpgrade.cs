using UnityEngine;

[CreateAssetMenu(menuName = "Player/Upgrades/NewAux")]
public class AuxUpgrade : UpgradeBase
{
    public AuxUpgrade()
    {
        slot = UpgradeSlot.None;
    }
}
