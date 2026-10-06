using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Databases/NodeTypeDatabase")]
public class RoomTypeDatabase : ContentDatabase<NodeTypeData>
{
    public NodeTypeData GetStart() => All.First(t => t.isStart);
    public NodeTypeData GetEnd() => All.First(t => t.isEnd);

    public IEnumerable<NodeTypeData> GetFillable() => All.Where(t => !t.isStart && !t.isEnd);
}
