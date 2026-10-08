using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RunState
{
    public SectorMap map;
    private Player player;
    public RewardContext rewardContext;


    // Progress
    public string currentNodeId;
    public readonly List<string> pathTaken = new();

    public MapNode CurrentNode => currentNodeId != null ? map.GetNode(currentNodeId) : null;
    public int Seed => map.seed;
    public int chapterIndex => map.chapter;
    public int currentDepth => CurrentNode?.depth ?? 0;

    // Run
    public float difficultyScore;
    public int currency;

    public float runDurationTimer;
    public static event Action<float> onTimerUpdated;

    // Stats
    public int EnemiesKilled;
    public int DamageTaken;

    #region Creation / Loading

    public RunState(SectorMap map, Player player)
    {
        this.map = map;
        this.player = player;
        rewardContext = new(player.Upgrades);
    }

    public static RunState FromSave(RunSaveData save, SectorMap map, Player player)
    {
        var run = new RunState(map, player)
        {
            currentNodeId = save.currentNodeId,
            currency = save.currency,
            runDurationTimer = save.runDuration,

            EnemiesKilled = save.enemiesKilled,
            DamageTaken = save.damageTaken
        };

        run.pathTaken.AddRange(save.pathTaken);
        run.rewardContext.FromSave(save.rewards);

        return run;
    }

    public RunSaveData ToSave()
    {
        return new RunSaveData()
        {
            seed = map.seed,
            chapter = map.chapter,
            map = MapSaveData.ToSave(map),
            currentNodeId = currentNodeId,
            pathTaken = new List<string>(pathTaken),

            currency = currency,
            runDuration = runDurationTimer,
            enemiesKilled = EnemiesKilled,
            damageTaken = DamageTaken,

            rewards = rewardContext.ToSave(),
            player = player.PackPlayerState()
        };
    }


    #endregion

    public void EnterNode(MapNode node)
    {
        currentNodeId = node.id;
        map.currentNode = node;
        pathTaken.Add(node.id);
    }

    public void Tick(float dt)
    {
        runDurationTimer += dt;
        onTimerUpdated?.Invoke(runDurationTimer);
    }

    #region Loot

    public void GrantLoot(LootResult result)
    {
        switch (result.Item)
        {
            case MajorUpgrade m:
                player.Upgrades.GrantMajor(m);
                break;

            case AuxUpgrade a:
                player.Upgrades.GrantAux(a);
                break;

            case PlayerWeapon w:
                player.Combat.EquipRangedWeapon(w);
                rewardContext.ownedWeaponIds.Add(w.Id);
                break;

            case PlayerTool t:
                player.Tools.EquipTool(t);
                rewardContext.ownedToolIds.Add(t.Id);
                break;

            default:
                Debug.LogError("[RunState] Attempting to grant unknown loot type");
                break;
        }

        rewardContext.SeenItemIdsThisRun.Add(result.Item.Id);
    }

    #endregion

    #region Resource Adjustments

    public void RestoreAmmo(int amount)
    {
        player.Combat.RestoreAmmo(Mathf.Max(0, amount));
    }

    public void RestoreStructure(int amount)
    {
        player.Health.RestoreSegments(Mathf.Max(0, amount));
    }

    public void GrantCurrency(int amount)
    {
        currency = Math.Min(currency + amount, 9999);

        GameEvents.CurrencyChanged(currency);
    }

    public bool TryUseCurrency(int cost)
    {
        if (currency < cost) return false;

        currency = currency -= cost;
        GameEvents.CurrencyChanged(currency);

        // fire event for ui
        return true;
    }

    #endregion

    // Calculate Rating for run


}
