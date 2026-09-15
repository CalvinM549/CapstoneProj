using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

    public List<MapNode> connections2;

    public bool visited;
    public bool cleared;
}

public class RoomConnection
{
    public MapNode target;
    public bool isRequired;
}

