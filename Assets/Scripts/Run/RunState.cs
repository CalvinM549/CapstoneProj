using System;
using UnityEngine;

[Serializable]
public class RunState
{
    // Run
    public int RunSeed;
    public float difficultyScore;

    // Map
    public int chapterIndex;
    public RunMap map;
    public int roomsCleared;
    public Doorway lastDoorway; // used to re-load spawn pos
    // Draft things

    public float runDurationTimer;
    public float playerHeatTimer;
    public float maxHeatValue;

    // Rooms
    public float HeatLevel;

    // Stats
    public int EnemiesKilled;
    public int DamageTaken;

    public event Action<float> OnHeatTick;

    #region Creation / Loading

    public RunState(int seed, RunMap map)
    {
        RunSeed = seed;
        this.map = map;

        runDurationTimer = 0f;
        playerHeatTimer = 120f; // Change to variable start time??

        roomsCleared = 0;

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
        playerHeatTimer -= dt;
        if (playerHeatTimer <= 0)
        {
            // Fire Event
        }
        else
        {
            OnHeatTick?.Invoke(playerHeatTimer / maxHeatValue);
        }
    }

    // Calculate Rating for run


}
