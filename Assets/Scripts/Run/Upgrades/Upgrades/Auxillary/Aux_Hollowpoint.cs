using UnityEngine;

// Opening shots: ranged damage bonus against enemies that are still mostly healthy.
[CreateAssetMenu(menuName = "Upgrades/Aux/Hollowpoint")]
public class Aux_Hollowpoint : AuxUpgrade
{
    [SerializeField] [Range(0f, 1f)] private float healthThreshold = 0.75f;
    [SerializeField] private float bonusPerStack = 0.25f;

    public override void ModifyHitOnTarget(HitData hit, EnemyController target)
    {
        if (hit.attackType != AttackType.Projectile || target == null) return;
        if (target.PercentHealth >= healthThreshold)
            hit.AddModifier(bonusPerStack * currentStacks, StatModType.PercentAdd, this);
    }
}
