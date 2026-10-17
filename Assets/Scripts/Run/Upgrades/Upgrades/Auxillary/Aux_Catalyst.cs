using UnityEngine;

// Enemies suffering two or more different statuses take extra damage from everything.
[CreateAssetMenu(menuName = "Upgrades/Aux/Catalyst")]
public class Aux_Catalyst : AuxUpgrade
{
    [SerializeField] private int requiredStatuses = 2;
    [SerializeField] private float bonusPerStack = 0.15f;

    public override void ModifyHitOnTarget(HitData hit, EnemyController target)
    {
        if (target == null || target.Statuses == null) return;

        if (target.Statuses.AllActiveEffects.Count >= requiredStatuses)
            hit.AddModifier(bonusPerStack * currentStacks, StatModType.PercentAdd, this);
    }
}
