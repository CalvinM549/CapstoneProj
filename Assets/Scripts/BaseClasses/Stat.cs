using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum StatModType
{
    Flat = 100,
    PercentAdd = 200,
    PercentMult = 300
}

[Serializable]
public class StatValue
{
    // Initial Value and Modifiers list
    private readonly float baseValue;
    private readonly List<StatModifier> statModifiers = new();

    // Temp values used for Value
    private bool isDirty = true;
    private float cachedValue;

    public event Action OnChanged;

    public StatValue(float baseValue)
    {
        this.baseValue = baseValue;
        cachedValue = baseValue;
    }

    // Value that is called by other scripts
    public float Value
    {
        get
        {
            if (isDirty) CalculateValue();
            return cachedValue;
        }
    }

    public float BaseValue => baseValue;

    // Adds or Removes new modifiers to the stat
    public void AddStatModifier(StatModifier modifier)
    {
        statModifiers.Add(modifier);
        SetDirty();
        //Debug.Log($"Added {modifier.value} as {modifier.type}");
    }

    public bool RemoveStatModifier(StatModifier modifier)
    {
        bool removed = statModifiers.Remove(modifier);
        if (removed)
        {
            SetDirty();
            //Debug.Log($"Removed {modifier}");
        }

        return removed;
    }

    public void RemoveAllFromSource(object source)
    {
        int removed = statModifiers.RemoveAll(m => m.source == source);
        if (removed > 0) SetDirty();
    }

    public void ClearAllModifiers()
    {
        if (statModifiers.Count == 0) return;
        statModifiers.Clear();
        SetDirty();
    }

    private void SetDirty()
    {
        isDirty = true;
        OnChanged?.Invoke();
    }

    // Calculates the final value of the stat, incl. modifiers
    private void CalculateValue()
    {

        float flat = BaseValue;
        float percentAdd = 0f;
        float percentMult = 1f;

        foreach (StatModifier mod in statModifiers)
        {
            switch (mod.type)
            {
                case StatModType.Flat: 
                    flat += mod.value; 
                    break;
                case StatModType.PercentAdd:
                    percentAdd += mod.value;
                    break;
                case StatModType.PercentMult:
                    percentMult *= mod.value;
                    break;
            }
        }

        cachedValue = flat * (1f + percentAdd) * percentMult;
        isDirty = false;
    }

}

[Serializable]
public class StatModifier
{
    public readonly float value;
    public readonly StatModType type;
    public readonly object source;

    public StatModifier(float value, StatModType type, object source = null)
    {
        this.value = value;
        this.type = type;
        this.source = source;
    }

    // Sets standard order, if it is not set
    public StatModifier(float value, StatModType type) : this(value, type, (int)type) { }

}