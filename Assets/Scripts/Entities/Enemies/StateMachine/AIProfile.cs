using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AIProfile")]
public class AIProfile : ScriptableObject
{
    [Header("Aggro")]
    public float aggroRange;
    [Tooltip("Blocks movement and pathing (walls and pits).")]
    public LayerMask obstacleLayer;
    [Tooltip("Blocks line of sight and aimed attacks (walls only, so enemies can see across pits). Falls back to obstacleLayer if empty.")]
    public LayerMask sightBlockLayer;
    public bool requireLineOfSight = true;

    public LayerMask SightMask => sightBlockLayer.value != 0 ? sightBlockLayer : obstacleLayer;

    [Header("Movement")]
    public float agentRadius = 0.3f;
    public float repositionDuration;
    public float repositionSpeedMult;

    public bool repositionAwayFromTarget = false;

    [Header("Combat")]
    public int[] attackPattern;
    public LayerMask targetLayer;

    [Header("AITick")]
    public float tickInterval;

}