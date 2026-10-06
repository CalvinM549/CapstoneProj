using System.Collections.Generic;
using UnityEngine;

public class MapNode
{
    public string id;
    public int depth;
    public int rowIndex;

    public Vector2Int coordinates;

    public RoomTypeData type;
    public RoomData room;

    public List<MapNode> exits = new();

    public ResolvedOffer[] offers;
    public string sectorCode;
}