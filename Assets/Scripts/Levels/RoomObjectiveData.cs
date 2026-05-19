using UnityEngine;

public enum RoomObjectiveType
{
    Clear, // Kill all enemies
    Gauntlet, // Survive Time
    Assassinate, // Kill target enemy
    Infiltrate // Hack console
}

[CreateAssetMenu(fileName = "NewObjective", menuName = "RoomObjective")]
public class RoomObjectiveData : ScriptableObject
{
    public RoomObjectiveType objectiveType;

    public string displayText;

    // KillAllEnemies

    // SurviveTime
    public float survivalDuration;

    // KillTargetEnemy
    public string targetEnemyTag;

    // UseTerminal
}
