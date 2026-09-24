using UnityEngine;

[CreateAssetMenu(menuName = "Map/NewGenerationConfig")]
public class MapGenerationConfig : ScriptableObject
{
    [Header("Zone generation settings")]
    public int roomCount;
    public int maxWidth;

    public NodeTypeConfig[] avaliableTypes;

    public float baseDifficulty = 1.0f;
    public float difficultyPerDepth = 0.3f;

    public int eliteMinGap = 2;
    public int maxElites = 3;

    // Room Generation
    [Range(0f, 1f)] public float shopChance = 0.3f;

    [Range(0f, 1f)] public float restRampProgress;
    [Range(0f, 1f)] public float eliteMinProgress;
    [Range(0f, 1f)] public float vaultMinProgress;

    // Loot
    public float weaponRewardProbability;
    public float toolRewardProbability;
    public float MajorRewardProbability;

}
