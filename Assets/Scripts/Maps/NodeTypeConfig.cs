using UnityEngine;

public enum RoomType
{
    unset,
    Start,
    Combat,
    Elite,
    Story,
    Vault,
    Shop,
    Rest,
    End
}

[CreateAssetMenu(menuName = "Map/NewNodeTypeConfig")]
public class NodeTypeConfig : ScriptableObject
{
    public RoomType type;
    public bool canRandomSpawn = true;

    public float baseWeight;

    public float minWeight;
    public float maxWeight;

    public float minProgress = 0f;

    public RewardCategory[] validRewards;
}
