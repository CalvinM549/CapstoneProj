using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EncounterService
{
    private EnemyDatabase db;
    private EncounterGenerationConfig config;

    private const int MaxSpawnAttemptsPerWave = 64;

    public EncounterService(EnemyDatabase db, EncounterGenerationConfig config)
    {
        this.config = config;
        this.db = db;
    }

    public EnemyWave[] GenerateWaves(RoomData room, RunState run)
    {
        List<EnemyData> pool = GetEligibleEnemies(run.chapterIndex);
        if (pool.Count <= 0)
        {
            Debug.LogWarning("[EncounterGenerationService] no eligable enemies for current chapter");
            return Array.Empty<EnemyWave>();
        }

        float totalBudget = Mathf.Max(config.baseBudget + (config.budgetPerDepth * run.currentDepth), 0);

        int waveCount = Mathf.Clamp(Mathf.RoundToInt(totalBudget / config.budgetPerWave), config.minWaves, config.maxWaves);
        float perWaveBudget = totalBudget / waveCount;

        // Get rng instance
        var rng = RNGManager.NodeRng(run.seed, run.map.currentNode.coordinates);

        var waves = new EnemyWave[waveCount];
        for (int i = 0; i < waveCount; i++)
        {
            float delay = i == 0 ? config.initialWaveDelay : config.interWaveDelay;
            waves[i] = BuildWave(pool, perWaveBudget, rng, delay);
        }

        return waves;
    }

    public EnemyWave BuildWave(List<EnemyData> pool, float budget, System.Random rng, float delay)
    {
        var wave = new EnemyWave()
        {
            startDelay = delay
        };

        var candidates = new List<EnemyData>(pool.Count);

        float remaining = budget;

        int attempts = 0;

        while (remaining > 0f && wave.enemies.Count < config.maxEnemiesPerWave && attempts < MaxSpawnAttemptsPerWave)
        {
            attempts++;

            candidates.Clear();
            foreach (var e in pool)
            {
                if (e.cost > budget) continue;
                candidates.Add(e);
            }

            if (candidates.Count == 0) break;

            var pick = WeightedPick(candidates, rng);
            wave.enemies.Add(pick);
            budget -= pick.cost;
        }

        return wave;
    }

    public List<EnemyData> GetEligibleEnemies(int chapter)
    {
        return db.All
            .Where(e => chapter >= e.minChapter && chapter <= e.maxChapter)
            .ToList();
    }

    static EnemyData WeightedPick(List<EnemyData> list, System.Random rng)
    {
        float total = 0;
        foreach (var e in list) total += e.weight;
        if (total <= 0) return list[rng.Next(list.Count)];

        float roll = (float)(rng.NextDouble() * total);
        foreach (var e in list)
        {
            roll -= e.weight;
            if (roll <= 0) return e;
        }
        return list[list.Count - 1];
    }
}
