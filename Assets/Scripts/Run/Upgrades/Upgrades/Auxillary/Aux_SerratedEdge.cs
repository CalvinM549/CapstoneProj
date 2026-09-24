using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Aux/StatusOnHit")]
public class Aux_SerratedEdge : AuxUpgrade
{
    [SerializeField] StatusEffectData statusData;
    [SerializeField] private int bleedStacksPerHit = 1;

    public override void ModifyOutgoingHit(HitData hit)
    {

        if (hit.attackType == AttackType.Light || hit.attackType == AttackType.Heavy)
        {
            hit.AddStatus(statusData, this, true, bleedStacksPerHit);
        }
    }

    
}
