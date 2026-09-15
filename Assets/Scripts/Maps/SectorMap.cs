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
    public int totalNodes;
    public int rowCount;

    public List<MapNode> GetConnections(MapNode node) => node.connections2;

    public List<MapNode> GetNodesAtDepth(int depth)
    {
        return nodes
            .Where(n => n.depth == depth)
            .ToList();
    }

    public MapNode GetNode(string id) => nodesByID.GetValueOrDefault(id);

}