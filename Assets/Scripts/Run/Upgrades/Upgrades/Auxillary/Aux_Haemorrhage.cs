using UnityEngine;

// Bleed ticks hit much harder on enemies that are already badly hurt.
[CreateAssetMenu(menuName = "Upgrades/Aux/Haemorrhage")]
public class Aux_Haemorrhage : AuxUpgrade
{
    [SerializeField] private StatusEffectData bleedStatus;
    [SerializeField] [Range(0f, 1f)] private float healthThreshold = 0.3f;
    [SerializeField] private float bonus = 1f; // +100% = double damage

    public override void ModifyHitOnTarget(HitData hit, EnemyController target)
    {
        if (hit.attackType != AttackType.DamageOverTime || target == null) return;
        if (bleedStatus == null || hit.sourceStatus != bleedStatus) return;

        if (target.PercentHealth < healthThreshold)
            hit.AddModifier(bonus, StatModType.PercentAdd, this);
    }
}
