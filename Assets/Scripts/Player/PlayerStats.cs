using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    //Movement
    MoveSpeed,
    DashSpeed,
    DashDuration,
    DashCharges,

    //Combat
    HeavyDamage,
    DsahDamage,
    AttackKnockback,
    
    //Health
    MaxHealthPerSegment,
    HealthSegmentCount,

    //Momentum
    MomentumCapacity,
    MomentumGainMult,
    MomentumDecayMult

}

public class PlayerStats : MonoBehaviour
{
    //[SerializeField] private PlayerStatsData data;

    private readonly Dictionary<StatType, StatValue> stats = new();

    public StatValue Get(StatType type) => stats[type];
    public float Value(StatType type) => stats[type].Value;

    private void Awake()
    {
        InitializeStats();
    }

    private void InitializeStats()
    {

    }

    public void ApplyTemporaryModifier(StatType stat, StatModifier modifier, float duration)
    {
        // Maybe Buff/Debuff Tracking

        StartCoroutine(HandleTemporaryModifier(stat, modifier, duration));
    }
    
    private IEnumerator HandleTemporaryModifier(StatType stat, StatModifier modifier, float duration)
    {
        //
        Get(stat).AddStatModifier(modifier);
        yield return new WaitForSeconds(duration);
        Get(stat).RemoveStatModifier(modifier);
        //
    }

}



