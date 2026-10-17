using UnityEngine;

// Melee damage scales with momentum: +2% per 10% momentum, per stack.
[CreateAssetMenu(menuName = "Upgrades/Aux/Inertial Edge")]
public class Aux_InertialEdge : AuxUpgrade
{
    [Tooltip("Melee bonus at full momentum, per stack.")]
    [SerializeField] private float bonusAtFullMomentum = 0.2f;

    public override void ModifyOutgoingHit(HitData hit)
    {
        if (player == null || hit.attackType is not (AttackType.Light or AttackType.Heavy)) return;

        float bonus = player.Momentum.PercentMomentum * bonusAtFullMomentum * currentStacks;
        if (bonus > 0.001f)
            hit.AddModifier(bonus, StatModType.PercentAdd, this);
    }
}
