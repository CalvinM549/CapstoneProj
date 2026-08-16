using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AIProfile")]
public class AIProfile : ScriptableObject
{
    [Header("Aggro")]
    public float aggroRange;
    public LayerMask obstacleLayer;
    public bool requireLineOfSight = true;

    [Header("Movement")]
    public float moveSpeed;
    public float repositionDuration;
    public float repositionSpeedMult;

    [Header("Combat")]
    public int[] attackPattern;
    public LayerMask targetLayer;

    [Header("AITick")]
    public float tickInterval;

}