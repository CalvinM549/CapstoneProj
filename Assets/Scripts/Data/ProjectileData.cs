using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "NewProjectileData", order = 1)]
public class ProjectileData : ScriptableObject
{
    public Projectile prefab;
    [Tooltip("How many instances should be made when requested")]
    public int poolSize;

    [Header("Movement")]
    public float lifetime;
    public float speed;

    [Header("Damage")]
    public int damage;
    public DamageType damageType = DamageType.Default;
    public float knockback;
    public float hitstunTime = 0.1f;
    public float hitstopDuration;

    [Space]
    public bool isParryable = true;

    [Header("VFX")]
    public GameObject impactVFXPrefab;
    public GameObject expireVFXPrefab;
}
