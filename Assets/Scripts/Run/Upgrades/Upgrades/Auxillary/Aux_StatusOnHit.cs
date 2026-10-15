using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Aux/StatusOnHit")]
public class Aux_StatusOnHit : AuxUpgrade
{
    [SerializeField] StatusEffectData statusData;

    [SerializeField][Range(0f, 1f)] private float procChance = 1f;
    [SerializeField] private int stacksPerHitPerStack = 1;

    [SerializeField] private AttackType[] applierTypes; 

    public override void ModifyOutgoingHit(HitData hit)
    {
        if (applierTypes.Contains(hit.attackType))
        {
            bool proc = Random.Range(0f, 1f) < procChance;

            if(proc)
                hit.AddStatus(statusData, this, true, (stacksPerHitPerStack * currentStacks));
        }
    }
}
