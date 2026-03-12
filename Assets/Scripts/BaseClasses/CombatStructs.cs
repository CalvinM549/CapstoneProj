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
}

public class DamageData
{
    public float damage;
    public AttackType attackType;
    public DamageType damageType;

    public Vector2 sourcePos;
    public Vector2 knockbackDirection;

    public float knockbackForce;
    public bool isPlayerAttack;

}

public class HitboxData
{
    public Vector2 offset;
    public Vector2 size;
    public AttackType attackType;
    public int comboStep;
}