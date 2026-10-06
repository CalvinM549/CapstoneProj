using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyService
{
    public EnemyDatabase enemyDatabase;

    private Dictionary<string, Stack<EnemyController>> pooledEnemies = new();
    private Transform container;

    public event Action OnAllEnemiesCleared;

    public EnemyService(EnemyDatabase data, Transform container)
    {
        enemyDatabase = data;
        this.container = container;

        Debug.Log("enemy service built");
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

    public EnemyController GetEnemy(EnemyData data, Vector2 position)
    {
        var stack = GetOrCreateStack(data.Id);

        EnemyController enemy = stack.Count > 0 ? stack.Pop() : CreateForPool(data);

        enemy.transform.position = position;
        enemy.transform.SetPositionAndRotation(position, Quaternion.identity);
        enemy.gameObject.SetActive(true);

        enemy.OnSpawn();

        enemy.OnDeath -= HandleEnemyDeath;
        enemy.OnDeath += HandleEnemyDeath;

        return enemy;
    }

    private void HandleEnemyDeath(EnemyController enemy)
    {
        ReturnToPool(enemy);
    }

    public void ReturnToPool(EnemyController enemy)
    {
        enemy.OnDeath -= HandleEnemyDeath;

        enemy.ResetForPool();
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(container, false);

        var stack = GetOrCreateStack(enemy.data.Id);
        stack.Push(enemy);
    }

    private EnemyController CreateForPool(EnemyData data)
    {
         return GameObject.Instantiate(data.prefab, container);
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
}

