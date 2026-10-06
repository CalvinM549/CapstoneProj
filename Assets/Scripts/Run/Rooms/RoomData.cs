using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Room Def")]
public class RoomData : DatabaseEntry, ICategorizedEntry<RoomType>
{
    public RoomType type;
    public NodeTypeData typeConfig;
    public RoomType Category => type;

    public DirectionMask allowedConnections = DirectionMask.All;

    public int difficultyCost;

    public RoomManager roomPrefab;
    // Rewards
    // Shop inventory

    // Used for restricting rooms per chapter
    public int minChapter = 0;
    public int maxChapter = 99;

    [Tooltip("Optional for hand-crafted rooms")]
    public EnemyWave[] encounterWaves;

    public bool SupportsMask(DirectionMask required) => (allowedConnections & required) == required;
}