using System.Collections.Generic;
using UnityEngine;

public class RoomPoolService
{
    private Transform roomContainer;

    public RoomPoolService(Transform container)
    {
        roomContainer = container;
    }

    public RoomManager Spawn(RoomData data)
    {
        return Object.Instantiate(data.roomPrefab, roomContainer);
    }

    public void Release(RoomManager room)
    {
        room.gameObject.SetActive(false);
        Object.Destroy(room.gameObject);
    }
}
