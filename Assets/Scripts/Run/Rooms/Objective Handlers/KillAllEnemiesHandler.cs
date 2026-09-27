using System;
using System.Security.Cryptography;
using UnityEngine;

public class KillAllEnemiesHandler : MonoBehaviour, IObjectiveTracker
{
    private EnemyService enemyService;
    private RoomManager room;

    public event Action OnEncounterCleared;
    public event Action<float> OnProgressChanged;

    private int startEnemies;
    private int currentEnemies;

    private int totalEnemies;
    private int enemiesKilled;

    private void Awake()
    {
        room = GetComponent<RoomManager>();
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyKilled -= HandleEnemyDeath;
    }

    public void Setup(RoomData room, RunState run)
    {
        enemyService = RunManager.Instance.enemyService;

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
