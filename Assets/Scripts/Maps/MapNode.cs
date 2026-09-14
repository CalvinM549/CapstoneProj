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

public class RoomConnection
{
    public MapNode target;
    public bool isRequired;
}

