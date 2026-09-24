using UnityEngine;

public interface IStatModifiable
{
    float Get(string statID);

    void ApplyStatModifier(string statID, StatModifier modifier);

    void RemoveStatModifier(string statID, StatModifier modifier);

    void RemoveModifiersFromSource(object source);
}
