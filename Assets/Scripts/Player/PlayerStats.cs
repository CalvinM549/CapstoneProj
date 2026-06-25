using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private Player p;

    // Stat List, contains muteable stats affected by the game in the form of upgrades etc

    //Movement

    //Combat

    //Momentum

    //Lockons

    //[SerializeField] private PlayerStatsData data;
    private readonly Dictionary<string, StatValue> stats = new();

    private StatValue Get(string statID)
    {
        stats.TryGetValue(statID, out StatValue value);
        if(value == null)
            return null;

        return value;
    }

    public float Value(string statID)
    {
        return stats[statID].Value;
    }

    private void Awake()
    {
        p = GetComponent<Player>();

        InitializeStats();
    }

    private void Start()
    {
        InitStat(p.Movement.CurrentMoveDirection.x);
    }

    private void InitStat(float test)
    {
        print(nameof(test));
    }

    private void InitializeStats()
    {

    }

    public void ApplyStatModifier(string statID, StatModifier modifier)
    {
        var stat = Get(statID);

        stat.AddStatModifier(modifier);
    }

    public void RemoveStatModifier(string statID, StatModifier modifier)
    {

        var stat = Get(statID);

        stat.RemoveStatModifier(modifier);
    }

    public void ApplyTemporaryModifier(string statID, StatModifier modifier, float duration)
    {
        // Maybe Buff/Debuff Tracking
        var stat = Get(statID);

        StartCoroutine(HandleTemporaryModifier(stat, modifier, duration));
    }
    
    private IEnumerator HandleTemporaryModifier(StatValue stat, StatModifier modifier, float duration)
    {
        //
        stat.AddStatModifier(modifier);
        yield return new WaitForSeconds(duration);
        stat.RemoveStatModifier(modifier);
        //
    }

}



