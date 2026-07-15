using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MapNode
{
    public RoomData room;
    public int nodeIndex;
    public int row;
    public int col;
    public List<MapNode> connections = new();
    public bool cleared;
}

public class RunMap
{
    public List<List<MapNode>> rows = new();
    public MapNode currentNode;
    public MapNode startNode;
}