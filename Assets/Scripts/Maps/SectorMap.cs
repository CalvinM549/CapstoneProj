using System.Collections.Generic;
using UnityEngine;

public class SectorMap
{
    public Dictionary<Vector2Int, MapNode> tiles = new();
    public MapNode currentNode;
    public MapNode startNode;

    public int totalNodes;
}