using UnityEngine;

public enum AttackRole
{
    Primary,
    Punish
}

[CreateAssetMenu(menuName = "Enemy/EnemyWeaponData")]
public class EnemyAttackData : ScriptableObject
{
    [Header("Selection")]
    public int priority;
    public AttackRole role = AttackRole.Primary;

    [Header("Config")]
    public float attackRange;
    public float approachSpeedMult = 1f;
    public float patienceDuration = -1f;

    public float telegraphDuration;
    public float winddownDuration;

    public bool hasHyperarmour;

    [Header("Animations")]
    public string windupTrigger;
    public string activeTrigger;
    [Tooltip("Typically only used for projectile executors")]
    public string winddownTrigger;

    [Header("Ranged")]
    public ProjectileData projectile;
    [Min(1)] public int projectileCount = 1;
    public float spreadAngle;
    public float shotInterval;

    [Header("Melee")]
    public AttackType attackType = AttackType.Heavy;
    public float baseDamage;

    public float knockbackForce;
    public float hitstunTime;
    public float hitstopTime;

    public bool isBlockable;
    public bool isParryable;
}
