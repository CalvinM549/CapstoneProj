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

[CreateAssetMenu(menuName = "Map/NewNodeTypeData")]
public class NodeTypeData : DatabaseEntry
{
    [Header("Display")]
    public string displayName;
    public bool isStart;
    public bool isEnd;
    public bool isCombat;
    public bool isShop;

    public RoomType type;
    public bool canRandomSpawn = true;


    [Header("Placement")]
    public float baseWeight = 1f;
    public AnimationCurve weightOverProgress = AnimationCurve.Constant(0f, 1f, 1f);

    public float minProgress = 0f;
    public float maxProgress = 0f;
    [Space]

    public int guarenteedCount;
    public float guarenteedMinProgress;


    [Header("Encounter")]
    public float encounterBudgetModifier = 0f;
    public int minWaves;
    public int maxWaves;


    [Header("Rewards")]
    public RewardCategory[] validRewards;
    public int baseOfferCount;

    public float GetWeight(float progress)
    {
        if (progress < minProgress || progress > maxProgress) return 0f;
        return baseWeight * weightOverProgress.Evaluate(progress);
    }
}
