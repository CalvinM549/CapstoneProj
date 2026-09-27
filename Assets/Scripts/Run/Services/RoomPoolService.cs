using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class RoomPoolService
{
    private RoomDatabase roomDatabase;
    private Transform roomContainer;

    private Dictionary<string, RoomManager> rooms;

    public RoomPoolService(RoomDatabase data, Transform container)
    {
        roomDatabase = data;
        roomContainer = container;
    }

    public void BuildStartRoom(SectorMap map)
    {
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
        if (!rooms.TryGetValue(node.id, out RoomManager entry))
        {
            Debug.LogWarning($"[RoomPoolService] failed to find room with id {node.id} in pool");
            return null;
        }

        entry.gameObject.SetActive(true);
        return entry;
    }

    public void ReturnToPool(RoomManager room)
    {
        room.gameObject.SetActive(false);
        return;
    }
}
