using UnityEngine;

[CreateAssetMenu(menuName = "Map/NewGenerationConfig")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Zone generation settings")]
    public int roomCount;
    public int maxWidth;

    public int maxExitsPerNode;

    public float baseDifficulty = 1.0f;
    public float difficultyPerDepth = 0.3f;

    [Header("Shard caches (combat sectors only)")]
    [Range(0f, 1f)] public float shardBaseChance = 0.2f;
    [Range(0f, 1f)] public float shardChancePerDepth = 0.03f;
}
