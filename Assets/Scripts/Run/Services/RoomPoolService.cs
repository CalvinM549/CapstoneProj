using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class RoomPoolService
{
    private RoomDatabase roomDatabase;
    private Transform roomContainer;

    private Dictionary<Vector2Int, RoomManager> pooledRooms;
    private Dictionary<string, RoomManager> rooms;
    //private List<RoomManager> pooledRooms;
    private RoomManager activeRoom;

    public RoomPoolService(RoomDatabase data, Transform container)
    {
        roomDatabase = data;
        roomContainer = container;
    }

    public void BuildPool(SectorMap map)
    {
        //pooledRooms = new Dictionary<Vector2Int, RoomManager>();

        //foreach (var node in map.tiles.Values)
        //{
        //    if (pooledRooms.ContainsKey(node.coordinates))
        //    {
        //        Debug.LogError($"[RoomPoolService] Duplicate node index found at {node.coordinates}");
        //        return;
        //    }

        //    var obj = GameObject.Instantiate(node.room.roomPrefab, roomContainer);
        //    // Determine save state? i.e. apply completion status
        //    obj.gameObject.SetActive(false);
        //    pooledRooms[node.coordinates] = obj;
        //}

        rooms = new();
        var roomObj = GameObject.Instantiate(map.entryNode.room.roomPrefab, roomContainer);
        roomObj.gameObject.SetActive(false);
        rooms[map.entryNode.id] = roomObj;

        BuildConnectedRooms(map.entryNode);
    }

    public void BuildConnectedRooms(MapNode current)
    {
        foreach (var node in current.connections2)
        {
            var roomObj = GameObject.Instantiate(node.room.roomPrefab, roomContainer);
            roomObj.gameObject.SetActive(false);
            rooms[node.id] = roomObj; 
        }
    }

    public RoomManager GetRoom(MapNode node)
    {
        //if (activeRoom != null) Debug.LogError("[RoomPoolService] existing room active, return old to pool first");

        //if (!pooledRooms.TryGetValue(node.coordinates, out RoomManager entry))
        //{
        //    Debug.LogWarning($"[RoomPoolService] failed to find room with id {node.coordinates} in pool");
        //    return null;
        //}

        //entry.gameObject.SetActive(true);
        //activeRoom = entry;
        //return entry;

        if (activeRoom != null) Debug.LogError("[RoomPoolService] existing room active, return old to pool first");

        if (!rooms.TryGetValue(node.id, out RoomManager entry))
        {
            Debug.LogWarning($"[RoomPoolService] failed to find room with id {node.id} in pool");
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
