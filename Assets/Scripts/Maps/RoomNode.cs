using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class RoomNode
{
    public RoomData room;
    public Vector2Int coordinates;
    public Dictionary<Direction, RoomNode> connections = new();
    public bool cleared;
}

public class RoomConnection
{
    public RoomNode target;
    public bool isRequired;
}

public class RunMap
{
    //public List<List<MapNode>> rows = new();
    public Dictionary<Vector2Int, RoomNode> tiles = new();
    public RoomNode currentNode;
    public RoomNode startNode;

    public int totalNodes;
}