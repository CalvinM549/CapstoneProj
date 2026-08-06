using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MapNode
{
    public RoomData room;
    public Vector2Int coordinates;
    public Dictionary<Direction, MapNode> connections = new();
    public bool cleared;
}

public class RunMap
{
    //public List<List<MapNode>> rows = new();
    public Dictionary<Vector2Int, MapNode> tiles = new();
    public MapNode currentNode;
    public MapNode startNode;

    public int totalNodes;
}