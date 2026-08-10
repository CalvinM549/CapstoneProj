using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class RoomPoolService
{
    private RoomDatabase roomDatabase;
    private Transform roomContainer;

    private Dictionary<Vector2Int, RoomManager> pooledRooms;
    //private List<RoomManager> pooledRooms;
    private RoomManager activeRoom;

    public RoomPoolService(RoomDatabase data, Transform container)
    {
        roomDatabase = data;
        roomContainer = container;
    }

    public void BuildPool(RunMap map)
    {
        pooledRooms = new Dictionary<Vector2Int, RoomManager>();

        foreach (var node in map.tiles.Values)
        {
            if (pooledRooms.ContainsKey(node.coordinates))
            {
                Debug.LogError($"[RoomPoolService] Duplicate node index found at {node.coordinates}");
                return;
            }

            var obj = GameObject.Instantiate(node.room.roomPrefab, roomContainer);
            // Determine save state? i.e. apply completion status
            obj.gameObject.SetActive(false);
            pooledRooms[node.coordinates] = obj;
        }
    }

    public RoomManager GetRoom(RoomNode node)
    {
        if (activeRoom != null) Debug.LogError("[RoomPoolService] existing room active, return old to pool first");

        if (!pooledRooms.TryGetValue(node.coordinates, out RoomManager entry))
        {
            Debug.LogWarning($"[RoomPoolService] failed to find room with id {node.coordinates} in pool");
            return null;
        }

        entry.gameObject.SetActive(true);
        activeRoom = entry;
        return entry;
    }

    public void ReturnToPool(RoomManager room)
    {
        room.gameObject.SetActive(false);
        activeRoom = null;
        return;
    }
}
