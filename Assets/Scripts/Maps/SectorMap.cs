using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SectorMap
{
    public Dictionary<Vector2Int, MapNode> tiles = new();

    public List<MapNode> nodes = new();
    public Dictionary<string, MapNode> nodesByID = new();

    public MapNode entryNode;
    public MapNode currentNode;

    public int seed;
    public int chapter;
    public int totalNodes;
    public int rowCount;

}