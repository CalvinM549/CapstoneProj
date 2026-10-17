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
    private Player p;

    [SerializeField] private MomentumData data;

    private float currentMomentum = 0f;
    private MomentumZone currentZone = MomentumZone.Empty;

    private StatValue gainMult;
    private StatValue drainMult;

    private float momentumTimer;
    private bool passiveDrainActive = false;

    public float Capacity => data.capacity;
    public float CurrentMomentum => currentMomentum;
    public float PercentMomentum => currentMomentum / data.capacity;
    public MomentumZone CurrentZone => currentZone;

    public void Initialize()
    {
        gainMult = p.Stats.GetStatValue(StatRef.PlayerMomentumGainMult);
        drainMult = p.Stats.GetStatValue(StatRef.PlayerMomentumDrainMult);
    }

    public void Initialize(float saved)
    {
        Initialize();
        currentMomentum = saved;
    }

    private void Awake()
    {
        p = GetComponent<Player>();
    }

    private void OnEnable()
    {
        GameEvents.OnHitConfirmed += HandleHitConfirmed;
        GameEvents.OnEnemyKilled += HandleEnemyKilled;

        GameEvents.OnPlayerTookDamage += HandlePlayerHit;
        GameEvents.OnAttackWhiff += HandleAttackWhiff;
    }

    private void OnDisable()
    {
        GameEvents.OnHitConfirmed -= HandleHitConfirmed;
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;

        GameEvents.OnPlayerTookDamage -= HandlePlayerHit;
        GameEvents.OnAttackWhiff -= HandleAttackWhiff;
    }

    private void Update()
    {
        UpdatePassiveDrain();
    }

    public void AddMomentum(float amount)
    {
        if (amount <= 0) return;

        currentMomentum = Mathf.Clamp(currentMomentum + (amount * gainMult.Value), 0f, data.capacity);

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

        currentMomentum = Mathf.Clamp(currentMomentum - (amount * drainMult.Value), 0f, data.capacity);

        if (!passiveDrainActive)
        {
            passiveDrainActive = true;
            GameEvents.PassiveDrainStart();
        }

        CheckZoneTransition();
        GameEvents.MomentumChange(PercentMomentum);
    }

    public void SpendMomentumPercent(float percent)
    {
        if (percent <= 0f) return;

        currentMomentum = Mathf.Max(0f, currentMomentum - percent * data.capacity);

        CheckZoneTransition();
        GameEvents.MomentumChange(PercentMomentum);
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
            currentMomentum = Mathf.Max(0f, currentMomentum - (data.drainSpeed * Time.deltaTime));

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
            _ => data.lightAttackGain
        };

        AddMomentum(gain);
    }

    private void HandleEnemyKilled(EnemyController enemy)
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
        if (p.Upgrades != null && p.Upgrades.PreventsWhiffDrain(type)) return;

        float drainAmount = type switch
        {
            AttackType.Light => data.lightWhiffDrain, 
            AttackType.Heavy => data.heavyWhiffDrain,
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

    public float GetMomentumSpeedMultiplier()
    {
        float mult = 1f;
            
        switch (currentZone)
        {
            case MomentumZone.Empty:
            case MomentumZone.Low:
                mult = data.lowSpeedMult;
                break;

            case MomentumZone.Mid:
                mult = data.midSpeedMult;
                break;

            case MomentumZone.High:
            case MomentumZone.Full:
                mult = data.highSpeedMult;
                break;
        }

        return mult;
    }

    #endregion

}
