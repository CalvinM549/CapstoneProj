using UnityEngine;

public enum MomentumZone
{
    Empty,
    Low,
    Mid,
    High
}

public class MomentumSystem : MonoBehaviour
{
    [SerializeField] private MomentumData data;

    private float currentMomentum = 0f;
    private MomentumZone currentZone = MomentumZone.Empty;

    private float gainMultiplier = 1.0f;
    private float drainMultiplier = 1.0f;

    public float CurrentMomentum => currentMomentum;
    public float PercentMomentum => currentMomentum / data.capacity;
    public MomentumZone CurrentZone => currentZone;

    private void OnEnable()
    {
        GameEvents.OnHitConfirmed += HandleHitConfirmed;
        GameEvents.OnEnemyKilled += HandleEnemyKilled;

        GameEvents.OnPlayerHit += HandlePlayerHit;
        GameEvents.OnHeavyAttackWhiff += HandleHeavyWhiff;
    }

    private void OnDisable()
    {
        GameEvents.OnHitConfirmed -= HandleHitConfirmed;
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;

        GameEvents.OnPlayerHit -= HandlePlayerHit;
        GameEvents.OnHeavyAttackWhiff -= HandleHeavyWhiff;
    }

    public void AddMomentum(float amount)
    {
        if (amount <= 0) return;

        currentMomentum = Mathf.Clamp(currentMomentum + (amount * gainMultiplier), 0f, data.capacity);
        CheckZoneTransition();
        GameEvents.MomentumChange(PercentMomentum);
    }

    public void DrainMomentum(float amount)
    {
        if (amount <= 0) return;

        currentMomentum = Mathf.Clamp(currentMomentum - (amount * drainMultiplier), 0f, data.capacity);

        CheckZoneTransition();
        GameEvents.MomentumChange(PercentMomentum);
    }

    #region Event Handlers

    // Gainers

    private void HandleHitConfirmed(HitData hit)
    {
        if (!hit.isPlayerAttack) return;

        float gain = hit.attackType switch
        {
            AttackType.Light => data.lightAttackGain,
            AttackType.Heavy => data.heavyAttackGain,
            AttackType.DashAttack => data.dashAttackGain,
            _ => data.lightAttackGain
        };

        AddMomentum(gain);
    }

    private void HandleEnemyKilled(EnemyBase enemy)
    {
        AddMomentum(data.killGain);
    }

    // Drainers

    private void HandlePlayerHit(HitData hit)
    {
        DrainMomentum(data.hitTakenDrain);
    }

    private void HandleHeavyWhiff()
    {
        DrainMomentum(data.heavyWhiffDrain);
    }

    #endregion


    #region Utilities
    private void CheckZoneTransition()
    {
        MomentumZone newZone = PercentMomentum switch
        {
            var n when n >= data.highThreshold => MomentumZone.High,
            var n when n >= data.lowThreshold => MomentumZone.Mid,
            var n when n >= data.emptyThreshold => MomentumZone.Low,
            _ => MomentumZone.Empty
        };

        if (newZone == currentZone) return;

        MomentumZone oldZone = currentZone;
        currentZone = newZone;

        switch (currentZone)
        {
            case MomentumZone.High: GameEvents.EnterHighMomentum(); break;
            case MomentumZone.Mid: GameEvents.EnterMidMomentum(); break;
            case MomentumZone.Low: GameEvents.EnterLowMomentum(); break;
            case MomentumZone.Empty: GameEvents.EnterEmptyMomentum(); break;
        }
        GameEvents.MomentumZoneChange(oldZone, currentZone);
    }

    #endregion

}
