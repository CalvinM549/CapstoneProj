using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Databases/RoomDatabase")]
public class RoomDatabase : CategorizedContentDatabase<RoomData, RoomType>
{
    public IEnumerable<RoomData> GetCandidates(RoomType type, DirectionMask requiredConnections, int chapter)
    {
        foreach (var room in GetByCategory(type))
        {
            if (room == null) continue;

            if (!room.SupportsMask(requiredConnections)) continue;

            if (chapter < room.minChapter || chapter > room.maxChapter) continue;

            yield return room;
        }
    }
}