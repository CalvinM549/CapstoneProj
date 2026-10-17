using UnityEngine;

// Attacks started right after a dash are "predicted": bonus damage, and whiffing them costs no momentum.
[CreateAssetMenu(menuName = "Upgrades/Major/Brain/Predictive Routines")]
public class Brain_PredictiveRoutines : MajorUpgrade
{
    [Tooltip("Seconds after a dash ends in which a new attack is empowered.")]
    [SerializeField] private float window = 0.35f;
    [SerializeField] private float damageBonus = 0.6f;
    [Tooltip("How long an empowered attack stays empowered (covers startup + active).")]
    [SerializeField] private float attackLifetime = 0.8f;

    private float lastDashEndTime = float.NegativeInfinity;
    private float empoweredStartTime = float.NegativeInfinity;
    private AttackType empoweredType;

    protected override void OnApply(Player player)
    {
        base.OnApply(player);
        GameEvents.OnPlayerDashEnd += HandleDashEnd;
        GameEvents.OnAttackStarted += HandleAttackStarted;
    }

    protected override void OnRemove(Player player)
    {
        base.OnRemove(player);
        GameEvents.OnPlayerDashEnd -= HandleDashEnd;
        GameEvents.OnAttackStarted -= HandleAttackStarted;
    }

    private void HandleDashEnd() => lastDashEndTime = Time.time;

    private void HandleAttackStarted(AttackType type, Vector2 direction)
    {
        if (Time.time - lastDashEndTime > window) return;

        empoweredType = type;
        empoweredStartTime = Time.time;
    }

    private bool IsEmpowered(AttackType type) => type == empoweredType && Time.time - empoweredStartTime <= attackLifetime;

    public override void ModifyOutgoingHit(HitData hit)
    {
        if (IsEmpowered(hit.attackType))
            hit.AddModifier(damageBonus, StatModType.PercentAdd, this);
    }

    public override bool PreventsWhiffDrain(AttackType type) => IsEmpowered(type);
}
