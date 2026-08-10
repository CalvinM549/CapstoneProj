using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private Player p;

    [SerializeField] private MovementData movementData;
    [SerializeField] private HealthData healthData;
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private MomentumData momentumData;

    private readonly Dictionary<string, StatValue> stats = new();


    private void Awake()
    {
        p = GetComponent<Player>();
    }

    public void Initialize()
    {
        // Movement
        Register(StatRef.PlayerBaseSpeed, 1f);
        Register(StatRef.PlayerDashCharges, 2f);
        Register(StatRef.PlayerDashRecharge, 1f);


        // Health
        Register(StatRef.PlayerBaseStructureHealth, 8);

        // Combat
        Register(StatRef.PlayerBaseMeleeDamage, 1f);
        Register(StatRef.PlayerBaseRangedDamage, 1f);
        Register(StatRef.PlayerBaseGlobalDamage, 1f);


        // Momentum

        // Heat
    }

    #region Helpers

    public float Get(string statID)
    {
        return stats.TryGetValue(statID, out StatValue sv) ? sv.Value : 0;
    }

    public int GetInt(string statID)
    {
        return stats.TryGetValue(statID, out StatValue sv) ? sv.ValueInt : 0;
    }

    public StatValue GetStatValue(string statID)
    {
        return stats.TryGetValue(statID, out StatValue sv) ? sv : null;
    }

    public void Subscribe(string statID, Action callback)
    {
        if(stats.TryGetValue(statID, out StatValue sv))
            sv.OnChanged += callback;
    }

    public void Unsubscribe(string statID, Action callback)
    {
        if (stats.TryGetValue(statID, out StatValue sv))
            sv.OnChanged -= callback;
    }


    public void ApplyStatModifier(string statID, StatModifier modifier)
    {
        if (!stats.TryGetValue(statID, out StatValue sv))
        {
            Debug.LogWarning($"[PlayerStats] stat '{statID}' not found, cannot add modifier");
            return;
        }

        sv.AddStatModifier(modifier);
    }

    public void RemoveStatModifier(string statID, StatModifier modifier)
    {

        if(stats.TryGetValue(statID, out StatValue sv))
            sv.RemoveStatModifier(modifier);
    }

    public void RemoveModifiersFromSource(object source)
    {
        foreach (StatValue sv in stats.Values)
            sv.RemoveAllFromSource(source);
    }

    public void ApplyTemporaryModifier(string statID, StatModifier modifier, float duration)
    {
        StartCoroutine(TemporaryModifierRoutine(statID, modifier, duration));
    }


    #endregion


    #region Utility

    private void Register(string id, float baseValue)
    {
        if (stats.ContainsKey(id))
        {
            Debug.LogWarning($"[PlayerStats] stat with id '{id}' already exists");
            return;
        }
        stats[id] = new StatValue(baseValue);
    }

    private IEnumerator TemporaryModifierRoutine(string statID, StatModifier modifier, float duration)
    {
        ApplyStatModifier(statID, modifier);
        // Ping event for UI

        yield return new WaitForSeconds(duration);
        RemoveStatModifier(statID, modifier);
    }

    #endregion

}



