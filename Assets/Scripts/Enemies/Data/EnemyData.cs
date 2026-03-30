using UnityEngine;


[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemies/NewEnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("Name")]
    public string enemyName;

    [Space]
    public float maxHealth;
    public float staggerThreshold;
    public float knockbackMultiplier;


    [Space]
    public float moveSpeed;
    public float acceleration;

    [Header("AI")]
    public float detectionRadius;
}
