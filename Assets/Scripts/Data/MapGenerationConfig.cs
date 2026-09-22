using UnityEngine;

[CreateAssetMenu(menuName = "Map/NewGenerationConfig")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Zone generation settings")]
    public Vector2Int zoneSize;
    [Range(0f, 1f)] public float extraRoomConnectionChance;

    public int roomCount;
    public int walkerCount;

    public float baseDifficulty = 1.0f;
    public float difficultyPerDepth = 0.3f;

    // Room Generation
    public int rewardRoomCount;
    [Range(0f, 1f)] public float shopChance = 0.3f;

    // Loot
    public float weaponRewardProbability;
    public float toolRewardProbability;
    public float MajorRewardProbability;

}
