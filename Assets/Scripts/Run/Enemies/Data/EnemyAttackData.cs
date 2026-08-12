using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyWeaponData")]
public class EnemyAttackData : ScriptableObject
{
    public float attackRange;
    public float telegraphDuration;
    public float winddownDuration;
}
