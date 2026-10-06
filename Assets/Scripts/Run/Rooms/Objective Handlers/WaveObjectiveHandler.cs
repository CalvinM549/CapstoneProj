using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveObjectiveHandler : MonoBehaviour, IObjectiveTracker
{
    private EnemyService enemyService;
    private RoomManager room;

    private EnemyWave[] waves;
    private int waveIndex;

    private bool waitingForNextWave;
    private float waveTimer;

    private readonly List<EnemyController> activeEnemies = new();

    private int totalEnemies;
    private int enemiesKilled;

    public event Action OnEncounterCleared;
    public event Action<float> OnProgressChanged;

    public event Action<int, int> OnWaveStart; // index, totalWaves;

    private void Awake()
    {
        room = GetComponent<RoomManager>();
    }

    public void Setup(RoomData room, RunState run)
    {
        enemyService = RunManager.Instance.enemyService;

        waves = (room.encounterWaves != null && room.encounterWaves.Length > 0)
            ? room.encounterWaves
            : RunManager.Instance.encounterService.GenerateWaves(room, run);

        waveIndex = -1;
        waitingForNextWave = false;
        activeEnemies.Clear();

        enemiesKilled = 0;
        totalEnemies = 0;

        if (waves != null)
        {
            foreach (var wave in waves)
                totalEnemies += wave.enemies.Count;
        }

        if (waves == null || waves.Length == 0)
        {
            Debug.Log("[WaveObjectiveHandler] No waves configured");
            return;
        }

        OnProgressChanged?.Invoke(0f);

        waitingForNextWave = true;
        waveTimer = Mathf.Max(0f, waves[0].startDelay);
    }

    public void Tick(float dt)
    {
        if (!waitingForNextWave) return;

        waveTimer -= dt;
        if (waveTimer <= 0f)
        {
            waitingForNextWave = false;
            SpawnWave(waveIndex + 1);
        }
    }

    private void SpawnWave(int index)
    {
        print($"Spawning Wave : {index}");

        waveIndex = index;
        var wave = waves[waveIndex];

        OnWaveStart?.Invoke(index, waves.Length);

        foreach (EnemyData entry in wave.enemies)
        {
            Transform spawnPoint = room.GetEnemySpawnPoint(entry.spawnTag);
            Vector2 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

            EnemyController enemy = enemyService.GetEnemy(entry, spawnPos);
            enemy.OnDeath += HandleEnemyDeath;
            activeEnemies.Add(enemy);


        }
    }

    private void HandleEnemyDeath(EnemyController enemy)
    {
        enemy.OnDeath -= HandleEnemyDeath;
        activeEnemies.Remove(enemy);

        enemiesKilled++;
        if (totalEnemies > 0)
            OnProgressChanged?.Invoke(enemiesKilled / totalEnemies);

        if (activeEnemies.Count > 0) return;

        bool hasNextWave = waveIndex + 1 < waves.Length;

        if (hasNextWave)
        {
            waitingForNextWave = true;
            waveTimer = Mathf.Max(0f, waves[waveIndex + 1].startDelay);
        }
        else
        {
            OnEncounterCleared?.Invoke();
        }
    }
}
