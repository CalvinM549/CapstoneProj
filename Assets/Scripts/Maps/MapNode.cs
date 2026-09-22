using System.Collections.Generic;
using UnityEngine;

public class MapNode
{
    public string id;
    public int depth;
    public int rowIndex;

    public RoomType type;
    public RoomData room;

    public Vector2Int coordinates;
    public Dictionary<Direction, MapNode> connections = new();

    public List<MapNode> connections2 = new();

    public bool visited;
    public bool cleared;

    public NodeRewards rewards;
    public List<LootResult> cachedLoot;
}

public class RoomConnection
{
    public MapNode target;
    public bool isRequired;
}

public struct NodeRewards
{
    public RewardCategory category;
    public int baseOfferCount;
    public bool isShop;
}