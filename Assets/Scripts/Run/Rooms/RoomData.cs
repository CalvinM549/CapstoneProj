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

[Flags]
public enum RoomTag
{
    None = 0,
    Melee = 1 << 0,
    Ranged = 1 << 1,
    Swarm = 1 << 2,
    Turret = 1 << 3,
    OpenArena = 1 << 4,
    Narrow = 1 << 5,
    Environmental = 1 << 6,
}

[CreateAssetMenu(menuName = "Run/Room Def")]
public class RoomData : DatabaseEntry, ICategorizedEntry<RoomType>
{
    public RoomType type;
    public RoomType Category => type;

    public RoomTag tag;

    public int difficultyCost;
    public float timerCost; // Expected time taken

    public RoomManager roomPrefab;
    // Rewards

    // Used for restricting rooms per chapter
    public int minChapter = 0;
    public int maxChapter = 99;

}
