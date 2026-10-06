using UnityEngine;

[CreateAssetMenu(menuName = "Map/NewEncounterConfig")]
public class EncounterGenerationConfig : ScriptableObject
{
    [Header("Budget")]
    public int baseBudget;
    public float budgetPerDepth;
    public AnimationCurve depthCurve = AnimationCurve.Linear(0, 1, 1, 1);

    [Header("Waves")]
    public int minWaves;
    public int maxWaves;

    public float initialWaveDelay;
    public float interWaveDelay;

    public float budgetPerWave;
    public int maxEnemiesPerWave;
}
