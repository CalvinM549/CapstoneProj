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
    public float baseValue;
    private readonly List<StatModifier> statModifiers;

    // Temp values used for Value
    private bool dirtyValue = true;
    private float _value;
    private float lastBaseValue = float.MinValue;

    // Value that is called by other scripts
    public float Value
    {
        get
        {
            if (dirtyValue || lastBaseValue != baseValue)
            {
                lastBaseValue = baseValue;
                _value = CalculateValue();
                dirtyValue = false;
            }
            return _value;
        }
    }

    // Constructors to create new Stat Types
    // Valueless Constructor
    public StatValue()
    {
        statModifiers = new List<StatModifier>();
    }
    // Constructor with preset base value
    public StatValue(float baseValue) : this()
    {
        this.baseValue = baseValue;
    }

    // Adds or Removes new modifiers to the stat
    public void AddStatModifier(StatModifier modifier)
    {
        dirtyValue = true;
        statModifiers.Add(modifier);

        Debug.Log($"Added {modifier.value} as {modifier.type}");
        // Resorts the list based on the correct order of modifiers
        statModifiers.Sort(CompareModifierOrder);
    }

    public bool RemoveStatModifier(StatModifier modifier)
    {
        dirtyValue = true;

        Debug.Log($"Removed {modifier}");
        return statModifiers.Remove(modifier);
    }

    private int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        // If the first modifier comes before the second
        if (a.order < b.order)
            return -1;
        // if the second modifier comes first
        else if (a.order > b.order)
            return 1;
        // if both modifiers are at the same point in the order
        return 0;
    }

    // Calculates the final value of the stat, incl. modifiers
    private float CalculateValue()
    {
        float finalValue = baseValue;
        float totalPercentAdd = 0;

        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier currentMod = statModifiers[i];

            // if a simple flat value is added
            if (currentMod.type == StatModType.Flat)
                finalValue += currentMod.value;

            // if an additive multiplier is added
            else if (currentMod.type == StatModType.PercentAdd)
            {
                // Adds the percent value to add to a single value
                totalPercentAdd += currentMod.value;

                // When it reaches the final percent add modifier, multiplies the final value and resets the counter
                if (i + 1 >= statModifiers.Count || statModifiers[i + 1].type != StatModType.PercentAdd)
                {
                    finalValue *= 1 + totalPercentAdd;
                    totalPercentAdd = 0;
                }
            }

            // if a multiplicative multiplier is added.
            else if (currentMod.type == StatModType.PercentMult)
                finalValue *= 1 + currentMod.value;
        }

        return Mathf.Round(finalValue);
    }

}

[Serializable]
public class StatModifier
{
    public readonly float value;
    public readonly StatModType type;
    public readonly int order;

    public StatModifier(float value, StatModType type, int order)
    {
        this.value = value;
        this.type = type;
        this.order = order;
    }

    // Sets standard order, if it is not set
    public StatModifier(float value, StatModType type) : this(value, type, (int)type)
    {

    }

}