using UnityEngine;

public abstract class StatController : MonoBehaviour, IStatModifiable
{
    public void ApplyStatModifier(string statID, StatModifier modifier)
    {
        throw new System.NotImplementedException();
    }

    public float Get(string statID)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveModifiersFromSource(object source)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveStatModifier(string statID, StatModifier modifier)
    {
        throw new System.NotImplementedException();
    }
}
