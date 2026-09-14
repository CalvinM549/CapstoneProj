using System;
using UnityEngine;

public enum RoomType
{
    Start,
    Combat,
    Elite,
    Hazard,
    Reward,
    Shop,
    Rest,
    Boss
}


[CreateAssetMenu(menuName = "Map/Room Def")]
public class RoomData : DatabaseEntry, ICategorizedEntry<RoomType>
{
    public RoomType type;
    public RoomType Category => type;

    public DirectionMask allowedConnections = DirectionMask.All;

    public int difficultyCost;
    public float timerCost; // Expected time taken

    public RoomManager roomPrefab;
    // Rewards

    // Used for restricting rooms per chapter
    public int minChapter = 0;
    public int maxChapter = 99;

    public bool CanConnectDirection(Direction dir) => (allowedConnections & dir.ToMask()) != 0;

    public bool SupportsMask(DirectionMask required) => (allowedConnections & required) == required;
}
