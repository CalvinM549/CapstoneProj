using System;
using UnityEngine;

[Serializable]
public class RunState
{
    // Run
    public int seed;

    public float difficultyScore;
    public int currency;

    private Player player;
    public RewardContext rewardContext;

    // Map
    public int chapterIndex;
    public SectorMap map;
    public int currentDepth;
    public Doorway lastDoorway; // used to re-load spawn pos

    public int runDirection;
    
    
    // Draft things

    public float runDurationTimer;

    // Stability
    public float stabilityTimer;
    public float maxStability = 120f;

    public bool StabilityDepleted => stabilityTimer <= 0f;

    public event Action<float> OnStabilityTick;
    public event Action OnStabilityDepleted;

    private bool stabilityDepletedFired;

    // Rooms

    // Stats
    public int EnemiesKilled;
    public int DamageTaken;

    #region Creation / Loading

    public RunState(int seed, SectorMap map, Player player)
    {
        this.seed = seed;
        this.map = map;

        this.player = player;

        this.rewardContext = new(player.Upgrades);

        runDurationTimer = 0f;
        
        // Run Stats reset
        currentDepth = 0;

        EnemiesKilled = 0;
        DamageTaken = 0;
    }

    public static RunState BuildFromSave(RunSaveData save)
    {
        // Rebuild map using seed
        // clear already cleared rooms

        //var run = new RunState(save.seed, map);

        //return run;

        return null; // TEMP

    }

    #endregion

    public void Tick(float dt)
    {
        runDurationTimer += dt;

        if (stabilityDepletedFired) return;

        if (stabilityTimer <= 0)
        {
            stabilityTimer = 0f;
            stabilityDepletedFired = true;
            OnStabilityDepleted?.Invoke();
            return;
        }

        OnStabilityTick?.Invoke(stabilityTimer / maxStability);
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

    public void DepleteStability(float amount)
    {
        stabilityTimer = Mathf.Max(0f, stabilityTimer - amount);
    }

    public void RestoreStability(float amount, bool fullRestore)
    {
        stabilityTimer = fullRestore ? maxStability : Mathf.Min(maxStability, stabilityTimer + amount);
    }

    public void RestoreAmmo(int amount)
    {
        player.Combat.RestoreAmmo(Mathf.Max(0, amount));
    }

    public void RestoreStructure(int amount)
    {
        player.Health.RestoreSegments(Mathf.Max(0, amount));
    }

    public void GrantPlayerUpgrade()
    {
        
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
