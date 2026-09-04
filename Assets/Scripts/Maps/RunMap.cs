using System.Collections.Generic;
using UnityEngine;

public class RunMap
{
    public Dictionary<Vector2Int, RoomNode> tiles = new();
    public RoomNode currentNode;
    public RoomNode startNode;

    public int totalNodes;
}