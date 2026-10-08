using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
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

    public EnemyWave[] GenerateWaves(MapNode node, RunState run)
    {
        List<EnemyData> pool = GetEligibleEnemies(run.chapterIndex);
        if (pool.Count <= 0)
        {
            Debug.LogWarning("[EncounterGenerationService] no eligable enemies for current chapter");
            return Array.Empty<EnemyWave>();
        }

        float totalBudget = Mathf.Max(config.baseBudget + (config.budgetPerDepth * run.currentDepth), 0);

        float budget = (config.baseBudget + config.budgetPerDepth * node.depth + node.room.difficultyCost)
            * node.type.encounterBudgetModifier;

        int minWaves = node.type.minWaves > 0 ? node.type.minWaves : config.minWaves;
        int maxWaves = node.type.maxWaves > 0 ? node.type.maxWaves : config.maxWaves;

        int waveCount = Mathf.Clamp(Mathf.RoundToInt(totalBudget / config.budgetPerWave), minWaves, maxWaves);
        float perWaveBudget = totalBudget / waveCount;

        // Get rng instance
        var rng = RNGManager.NodeRng(run.Seed, run.CurrentNode.coordinates);

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
                if(e.cost <= remaining) candidates.Add(e);

            if (candidates.Count == 0) break;

            var pick = WeightedPick(candidates, rng);
            wave.enemies.Add(pick);

            remaining -= Mathf.Max(pick.cost, 0.01f);
        }

        if (wave.enemies.Count == 0)
            wave.enemies.Add(pool.OrderBy(e => e.cost).First());

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
