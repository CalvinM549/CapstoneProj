using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyWeaponData")]
public class EnemyAttackData : ScriptableObject
{
    [Header("Config")]
    public float attackRange;
    public float telegraphDuration;
    public float winddownDuration;

    public bool hasHyperarmour;

    [Header("Animations")]
    public string windupTrigger;
    public string activeTrigger;

    [Header("Ranged")]
    public Projectile projectile;

    [Header("Melee")]
    public AttackType attackType = AttackType.Heavy;
    public float baseDamage;

    public float knockbackForce;
    public float hitstunTime;
    public float hitstopTime;

    public bool isBlockable;
    public bool isParryable;
}
