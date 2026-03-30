using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "NewProjectileData", order = 1)]
public class ProjectileData : ScriptableObject
{
    public float lifetime;
    public float speed;

    [Header("Damage")]
    public int damage;
    public float knockback;
    public float hitstopDuration;
    public DamageType damageType = DamageType.Default;

    [Space]
    public bool isParryable = true;

    [Header("VFX")]
    public GameObject impactVFXPrefab;
}
