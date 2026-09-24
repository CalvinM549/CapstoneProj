using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum AttackDirection
{
    Side,
    Up,
    Down
}

public enum AttackType
{
    Light,
    Heavy,
    Projectile,
    DamageOverTime,
    Elemental
}

public enum DamageType // Mainly for death effects?
{
    Default,
    Explosive,
    Electric
}

public enum CombatState
{
    Idle,
    Startup,
    Active,
    Recovery
}

public enum PushDirections
{
    InputDir,
    AimDir,
    AimInvert
}

public class HitData
{
    public AttackType attackType;
    public DamageType damageType;

    public Vector2 sourcePos;
    public Vector2 knockbackDirection;

    public float knockbackForce;
    public float hitstopTime;
    public float hitstunTime;

    public int momentumCost;

    public bool isPlayerAttack;
    public bool isParryable;
    public bool isBlockable;

    public float BaseDamage { get; }
    public bool IsResolved { get; private set; }
    public float FinalDamage
    {
        get
        {
            if (!IsResolved)
                return Resolve();
            else
                return _finalDamage;
        }
    }

    private float _finalDamage;


    private readonly List<StatModifier> modifiers = new();

    private List<PendingStatus> statusQueue = new();

    public IReadOnlyList<PendingStatus> PendingStatusEffects => (IReadOnlyList<PendingStatus>)statusQueue ?? Array.Empty<PendingStatus>();


    public HitData(float baseDamage, AttackType attackType, bool isPlayerAttack)
    {
        BaseDamage = baseDamage;
        this.attackType = attackType;
        this.isPlayerAttack = isPlayerAttack;
    }

    public void AddModifier(StatModifier modifier)
    {
        if (IsResolved)
        {
            Debug.LogWarning($"[HitData] Modifier from {modifier.source} added after resolve");
            return;
        }

        modifiers.Add(modifier);
    }

    public void AddModifier(float value, StatModType type, object source = null)
    {
        AddModifier(new StatModifier(value, type, source));
    }

    public void AddStatus(StatusEffectData data, object source, bool appliedByPlayer, int stacks = 1)
    {
        statusQueue ??= new();
        statusQueue.Add(new PendingStatus(data, source, appliedByPlayer, stacks));
    }

    public float Resolve()
    {
        if (IsResolved) return FinalDamage;

        float flat = BaseDamage;
        float percentAdd = 0f;
        float percentMult = 1f;

        foreach (var mod in modifiers)
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
                    percentMult *= (1f * mod.value);
                    break;
            }
        }

        float result = flat * (1f + percentAdd) * percentMult;

        _finalDamage = Mathf.Max(0f, result);
        IsResolved = true;
        return _finalDamage;
    }



}

public readonly struct PendingStatus
{
    public readonly StatusEffectData data;
    public readonly object source;
    public readonly bool appliedByPlayer;
    public readonly int stacks;

    public PendingStatus(StatusEffectData data, object source, bool appliedByPlayer, int stacks)
    {
        this.data = data;
        this.source = source;
        this.appliedByPlayer = appliedByPlayer;
        this.stacks = stacks;
    }
}

[Serializable]
public class AttackInfo
{
    public AttackType type;

    public float damage;
    public float startupTime;
    public float activeTime;
    public float recoveryTime;

    public float dashForce;

    public float hitstopDuration;
    public float knockback;
    public float hitstunTime;
}