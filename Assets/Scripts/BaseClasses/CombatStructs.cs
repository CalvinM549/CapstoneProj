using UnityEngine;

public enum AttackType
{
    Light,
    Heavy,
    DashAttack,
    Secondary
}

public enum DamageType
{

}

public enum CombatState
{

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