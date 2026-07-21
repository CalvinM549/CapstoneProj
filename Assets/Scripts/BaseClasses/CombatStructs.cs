using System;
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
    DashAttack,
    Secondary
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

public struct HitData
{
    public int damage;
    public int momentumCost;
    public AttackType attackType;
    public DamageType damageType;

    public bool isPlayerAttack;
    public bool isParryable;
    public bool momentumCanBlock;

    public Vector2 sourcePos;
    public Vector2 knockbackDirection;
    public float knockbackForce;

    public float hitstunTime;

    public float hitstopTime;
}

[Serializable]
public class AttackInfo
{
    public AttackType type;

    public int damage;
    public float startupTime;
    public float activeTime;
    public float recoveryTime;

    public float dashForce;

    public float hitstopDuration;
    public float knockback;
    public float hitstunTime;
}