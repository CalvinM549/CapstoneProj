using UnityEngine;

// Cancelling an attack's recovery (dash or next attack) refunds a little momentum.
[CreateAssetMenu(menuName = "Upgrades/Aux/Follow-Through")]
public class Aux_FollowThrough : AuxUpgrade
{
    [Tooltip("Fraction of momentum capacity refunded per stack.")]
    [SerializeField] private float momentumPerStack = 0.05f;

    protected override void OnApply(Player player) => GameEvents.OnRecoveryCancel += HandleRecoveryCancel;
    protected override void OnRemove(Player player) => GameEvents.OnRecoveryCancel -= HandleRecoveryCancel;

    private void HandleRecoveryCancel(AttackType type)
    {
        if (player != null)
            player.Momentum.AddMomentum(player.Momentum.Capacity * momentumPerStack * currentStacks);
    }
}
