using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum StatusStackMode
{
    RefreshDuration,
    Intensity,
    Independant
}

public enum StatusType
{
    Buff,
    Debuff,
    Neutral
}

public class ActiveStatusEffect
{
    public readonly StatusEffectData data;
    public readonly object applier;
    public readonly bool appliedByPlayer;

    public int stacks = 1;
    public float remainingDuration;
    public float tickTimer;

    public ActiveStatusEffect(StatusEffectData data, object applier, bool appliedByPlayer)
    {
        this.data = data;
        this.applier = applier;
        this.appliedByPlayer = appliedByPlayer;

        remainingDuration = data.duration;
        tickTimer = data.tickInterval;
    }
}

public abstract class StatusEffectData : ScriptableObject
{
    [Header("Display")]

    public string statusName;
    public Sprite icon;
    public StatusType type = StatusType.Debuff;

    [Header("Config")]

    public float duration;
    public StatusStackMode stackMode = StatusStackMode.RefreshDuration;
    [Min(1)] public int maxStacks;

    public List<UpgradeStatModifierEntry> statModifiers = new();

    public bool dealsDamageOverTime = false;
    public float amountPerTickPerStack = 0f;

    [Min(0.05f)] public float tickInterval = 1f;

    public virtual void OnApply(ActiveStatusEffect instance, GameObject target) { }

    public virtual void OnStack(ActiveStatusEffect instance, GameObject target) { }

    public virtual void OnTick(ActiveStatusEffect instace, GameObject target) { }

    public virtual void OnExpire(ActiveStatusEffect instance, GameObject target) { }
}
