using System.Collections.Generic;
using UnityEngine;

public class EnemyService
{
    public EnemyDatabase enemyDatabase;

    private Dictionary<string, Stack<EnemyController>> pooledEnemies = new();
    private Transform container;

    public EnemyService(EnemyDatabase data, Transform container)
    {
        enemyDatabase = data;
        this.container = container;
    }

    public void BuildPool(EnemyData data, int count)
    {
        var stack = GetOrCreateStack(data.Id);
        for (int i = 0; i < count; i++)
        {
            var obj = CreateForPool(data);
            obj.gameObject.SetActive(false);
            stack.Push(obj);
        }
    }

    // Wave Spawner
    // Enemy Spawner / Resetter

    public EnemyController GetEnemy(EnemyData data, Vector2 position) // Add type selector
    {
        var stack = GetOrCreateStack(data.Id);
        EnemyController enemy = stack.Count > 0 ? stack.Pop() : CreateForPool(data);

        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
        enemy.OnSpawn();
        return enemy;
    }

    public void ReturnToPool(EnemyController enemy)
    {
        enemy.ResetForPool();
        enemy.gameObject.SetActive(false);

        var stack = pooledEnemies[enemy.Data.Id];
        stack.Push(enemy);
    }

    private EnemyController CreateForPool(EnemyData data)
    {
        var obj = GameObject.Instantiate(data.prefab);
        return obj.GetComponent<EnemyController>();
    }

    private Stack<EnemyController> GetOrCreateStack(string id)
    {
        if(!pooledEnemies.TryGetValue(id, out var stack))
        {
            stack = new Stack<EnemyController>();
            pooledEnemies[id] = stack;
        }

        return stack;
    }
    
    // Enemy Wave data object
    public void SpawnWave(SpawnWave wave)
    {
        foreach (WaveEntry entry in wave.entries)
        {
            // Get spawn point
            for (int i = 0; i < entry.count; i++)
            {
                GetEnemy(entry.enemyType, Vector2.zero); // Replace with proper spawn pos
            }
        }
    }

}

