using UnityEngine;

// Keeps far more of the dash-chain speed bonus (asset stat: dash speed retention), and that speed hits back:
// melee damage scales with how much speed bonus is currently built up.
[CreateAssetMenu(menuName = "Upgrades/Major/Propulsion/Vector Thrust")]
public class Propulsion_VectorThrust : MajorUpgrade
{
    [Tooltip("Melee damage bonus at the maximum speed bonus.")]
    [SerializeField] private float maxMeleeBonus = 0.5f;

    public override void ModifyOutgoingHit(HitData hit)
    {
        if (player == null || hit.attackType is not (AttackType.Light or AttackType.Heavy)) return;

        float bonus = player.Movement.SpeedBonusNormalized * maxMeleeBonus;
        if (bonus > 0.001f)
            hit.AddModifier(bonus, StatModType.PercentAdd, this);
    }
}
