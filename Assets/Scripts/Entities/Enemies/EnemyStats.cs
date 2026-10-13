using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour, IStatModifiable
{
    private readonly Dictionary<string, StatValue> stats = new();

    public void Initialize(EnemyData data, AIProfile profile)
    {
        stats.Clear();

        Register(StatRef.EnemyMaxHealth, data.baseHealth);
        Register(StatRef.EnemyBaseSpeed, data.moveSpeed);

        Register(StatRef.EnemyOutgoingDamageMult, data.baseDamageMult);
        Register(StatRef.EnemyIncomingDamageMult, data.baseResistanceMult);
    }

    public float Get(string statID)
    {
        return stats.TryGetValue(statID, out var sv) ? sv.Value : 0f;
    }

    public StatValue GetStatValue(string statID)
    {
        return stats.TryGetValue(statID, out var sv) ? sv : null;
    }

    public void ApplyStatModifier(string statID, StatModifier modifier)
    {
        if (!stats.TryGetValue(statID, out StatValue sv))
        {
            Debug.LogWarning($"[EnemyStats] stat '{statID}' not found on {name}, cannot add modifier");
            return;
        }

        sv.AddStatModifier(modifier);
    }

    public void RemoveStatModifier(string statID, StatModifier modifier)
    {
        if (stats.TryGetValue(statID, out StatValue sv))
            sv.RemoveStatModifier(modifier);
    }

    public void RemoveModifiersFromSource(object source)
    {
        foreach (StatValue sv in stats.Values)
            sv.RemoveAllFromSource(source);
    }


    private void Register(string id, float baseValue)
    {
        stats[id] = new StatValue(baseValue);
    }
}
