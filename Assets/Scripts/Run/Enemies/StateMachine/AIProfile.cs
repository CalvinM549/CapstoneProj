using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AIProfile")]
public class AIProfile : ScriptableObject
{
    public float aggroRange;

    public float repositionDuration;
    public float moveSpeed;

    public float tickInterval;

    public LayerMask targetLayer;
    public LayerMask obstacleLayer;
}