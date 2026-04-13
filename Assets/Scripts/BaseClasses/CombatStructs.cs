using UnityEngine;

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
}public class HitData
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
