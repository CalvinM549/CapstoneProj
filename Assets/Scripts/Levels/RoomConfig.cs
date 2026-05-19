using UnityEngine;

[CreateAssetMenu(fileName = "NewRoom", menuName = "Rooms/Room Config")]
public class RoomConfig : ScriptableObject
{
    public string roomId;
    public GameObject roomPrefab;

    public RoomObjectiveData objective;

    // Reward stuff
}
