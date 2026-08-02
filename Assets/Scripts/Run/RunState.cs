using UnityEngine;

public class RunState
{
    // Run
    public int RunSeed;

    // Map
    public int chapterIndex;
    public RunMap map;
    public int roomsCleared;
    public Doorway lastDoorway; // used to re-load spawn pos
    // Draft things

    public float runDurationTimer;
    public float playerHeatTimer;

    // Rooms
    public float HeatLevel;

    // Stats
    public int EnemiesKilled;
    public int DamageTaken;

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

    public void Tick(float dt)
    {
        runDurationTimer += dt;
        playerHeatTimer -= dt;
        if (playerHeatTimer <= 0)
        {
            // Fire Event
        }
    }

    public void HeatReset()
    {

    }

}
