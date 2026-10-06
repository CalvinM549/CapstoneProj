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
}
