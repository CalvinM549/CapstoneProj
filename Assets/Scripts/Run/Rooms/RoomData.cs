using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Room Def")]
public class RoomData : DatabaseEntry, ICategorizedEntry<RoomType>
{
    public RoomType type;
    public RoomType Category => type;

    public DirectionMask allowedConnections = DirectionMask.All;

    public int difficultyCost;

    public RoomManager roomPrefab;
    // Rewards
    // Shop inventory

    // Used for restricting rooms per chapter
    public int minChapter = 0;
    public int maxChapter = 99;

    public bool SupportsMask(DirectionMask required) => (allowedConnections & required) == required;
}