using UnityEngine;

public enum MomentumZone
{
    Empty,
    Low,
    Mid,
    High,
    Full
}

public class PlayerMomentum : MonoBehaviour
{
    [SerializeField] private MomentumData data;

    private float currentMomentum = 0f;
    private MomentumZone currentZone = MomentumZone.Empty;

    private float gainMultiplier = 1.0f;
    private float drainMultiplier = 1.0f;

    private float momentumTimer;
    private bool passiveDrainActive = false;

    public float CurrentMomentum => currentMomentum;
    public float PercentMomentum => currentMomentum / data.capacity;
    public MomentumZone CurrentZone => currentZone;

    private void OnEnable()
    {
        GameEvents.OnHitConfirmed += HandleHitConfirmed;
        GameEvents.OnEnemyKilled += HandleEnemyKilled;

        GameEvents.OnPlayerHit += HandlePlayerHit;
        GameEvents.OnAttackWhiff += HandleAttackWhiff;
    }

    private void OnDisable()
    {
        GameEvents.OnHitConfirmed -= HandleHitConfirmed;
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;

        GameEvents.OnPlayerHit -= HandlePlayerHit;
        GameEvents.OnAttackWhiff -= HandleAttackWhiff;
    }

    private void Update()
    {
        UpdatePassiveDrain();
    }

    public void AddMomentum(float amount)
    {
        if (amount <= 0) return;

        currentMomentum = Mathf.Clamp(currentMomentum + (amount * gainMultiplier), 0f, data.capacity);

        if (currentMomentum == data.capacity)
        {
            // Hit momentum cap
        }

        momentumTimer = data.timeUntilDrain;

        if (passiveDrainActive)
        {
            passiveDrainActive = false;
            GameEvents.PassiveDrainEnd();
        }

        CheckZoneTransition();
        GameEvents.MomentumChange(PercentMomentum);
    }

    public void DrainMomentum(float amount)
    {
        if (amount <= 0) return;

        currentMomentum = Mathf.Clamp(currentMomentum - (amount * drainMultiplier), 0f, data.capacity);

        if (!passiveDrainActive)
        {
            passiveDrainActive = true;
            GameEvents.PassiveDrainStart();
        }

        CheckZoneTransition();
        GameEvents.MomentumChange(PercentMomentum);
    }

    public float DrainAsBuffer(float momentumCost)
    {
        if (momentumCost <= 0) return 1.0f;

        if (currentMomentum >= momentumCost)
        {
            DrainMomentum(momentumCost);
            return 0.0f;
        }
        else if (currentMomentum > 0f)
        {
            float absorbed = currentMomentum / momentumCost;
            float bleedThrough = 1f - absorbed;
            DrainMomentum(currentMomentum);
            return bleedThrough;
        }
        else
            return 1.0f;

    }

    private void UpdatePassiveDrain()
    {
        if (momentumTimer > 0)
            momentumTimer -= Time.deltaTime;
        
        else if (!passiveDrainActive)
        {
            passiveDrainActive = true;
            GameEvents.PassiveDrainStart();
            // Fire Event
        }

        if (passiveDrainActive && currentMomentum > 0)
        {
            currentMomentum -= data.drainSpeed * Time.deltaTime;
            CheckZoneTransition();
            GameEvents.MomentumChange(PercentMomentum);
        }
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

    private void HandleAttackWhiff(AttackType type)
    {
        float drainAmount = type switch
        {
            AttackType.Light => data.lightWhiffDrain, 
            AttackType.Heavy => data.heavyWhiffDrain,
            AttackType.DashAttack => data.dashWhiffDrain,
            _ => data.lightWhiffDrain
        };

        DrainMomentum(drainAmount);
    }

    #endregion


    #region Utilities
    private void CheckZoneTransition()
    {
        MomentumZone newZone = PercentMomentum switch
        {
            var n when n >= 1.0f => MomentumZone.Full,
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
            case MomentumZone.Full: break;
            case MomentumZone.High: GameEvents.EnterHighMomentum(); break;
            case MomentumZone.Mid: GameEvents.EnterMidMomentum(); break;
            case MomentumZone.Low: GameEvents.EnterLowMomentum(); break;
            case MomentumZone.Empty: GameEvents.EnterEmptyMomentum(); break;
        }
        GameEvents.MomentumZoneChange(oldZone, currentZone);
    }

    #endregion

}
