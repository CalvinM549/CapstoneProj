using UnityEngine;

// The first hit landed shortly after a dash grants a burst of momentum.
[CreateAssetMenu(menuName = "Upgrades/Aux/Slipstream")]
public class Aux_Slipstream : AuxUpgrade
{
    [SerializeField] private float window = 0.5f;
    [Tooltip("Fraction of momentum capacity granted per stack.")]
    [SerializeField] private float momentumPerStack = 0.08f;

    private float lastDashEndTime = float.NegativeInfinity;
    private bool consumed = true;

    protected override void OnApply(Player player)
    {
        GameEvents.OnPlayerDashEnd += HandleDashEnd;
        GameEvents.OnPlayerHitLanding += HandleHitLanding;
    }

    protected override void OnRemove(Player player)
    {
        GameEvents.OnPlayerDashEnd -= HandleDashEnd;
        GameEvents.OnPlayerHitLanding -= HandleHitLanding;
    }

    private void HandleDashEnd()
    {
        lastDashEndTime = Time.time;
        consumed = false;
    }

    private void HandleHitLanding(HitData hit, EnemyController target)
    {
        if (consumed || player == null || hit.attackType == AttackType.DamageOverTime) return;
        if (Time.time - lastDashEndTime > window) return;

        consumed = true;
        player.Momentum.AddMomentum(player.Momentum.Capacity * momentumPerStack * currentStacks);
    }
}
