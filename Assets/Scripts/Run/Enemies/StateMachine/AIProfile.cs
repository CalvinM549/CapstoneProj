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
    public float repositionDuration;
    public float repositionSpeedMult;

    public bool repositionAwayFromTarget = false;

    [Header("Combat")]
    public int[] attackPattern;
    public LayerMask targetLayer;

    [Header("AITick")]
    public float tickInterval;

}