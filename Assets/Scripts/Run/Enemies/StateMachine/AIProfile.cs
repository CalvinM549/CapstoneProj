using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AIProfile")]
public class AIProfile : ScriptableObject
{
    public float aggroRange;

    public int[] attackPattern;
    public float attackRange;
    public float attackCooldown;
    public float attackWindupTime;
    public float moveSpeed;
    public float staggerDuration;

    public float tickInterval;

    public LayerMask targetLayer;
    public LayerMask obstacleLayer;
}
