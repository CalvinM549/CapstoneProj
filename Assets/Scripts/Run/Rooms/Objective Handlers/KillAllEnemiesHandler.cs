using System;
using System.Security.Cryptography;
using UnityEngine;

public class KillAllEnemiesHandler : MonoBehaviour, IObjectiveTracker
{
    public event Action OnEncounterCleared;
    public event Action<float> OnProgressChanged;

    private int startEnemies;
    private int currentEnemies;

    public void Reset()
    {
        //
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyKilled -= HandleEnemyDeath;
    }

    public void Setup(RoomData room, RunState run)
    {
        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        startEnemies = enemies.Length;
        currentEnemies = startEnemies;

        GameEvents.OnEnemyKilled += HandleEnemyDeath;
    }

    public void Tick(float dt)
    {
        //
    }

    private void HandleEnemyDeath(EnemyController enemy)
    {
        currentEnemies--;
        OnProgressChanged?.Invoke(1 - (currentEnemies / startEnemies));

        if (currentEnemies <= 0)
        {
            OnEncounterCleared?.Invoke();
        }
    }
}
