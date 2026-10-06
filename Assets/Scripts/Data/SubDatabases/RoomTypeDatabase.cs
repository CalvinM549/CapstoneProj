using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Databases/NodeTypeDatabase")]
public class RoomTypeDatabase : ContentDatabase<RoomTypeData>
{
    [SerializeField] private RoomTypeData fallbackType;
    public RoomTypeData FallbackType => fallbackType;
    public RoomTypeData GetStart() => All.First(t => t.isStart);
    public RoomTypeData GetEnd() => All.First(t => t.isEnd);

    public IEnumerable<RoomTypeData> GetFillable() => All.Where(t => !t.isStart && !t.isEnd);
}
