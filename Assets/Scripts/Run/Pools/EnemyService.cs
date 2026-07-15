using UnityEngine;

public class EnemyService
{
    public EnemyDatabase enemyDatabase;

    public EnemyService(EnemyDatabase data)
    {
        enemyDatabase = data;
    }

    public void BuildPool()
    {
        // Either X enemies in each of Y pools for each type
        // Or All enemies in 1 dict with key
    }

    // Wave Spawner
    // Enemy Spawner / Resetter

    public EnemyController GetEnemy() // Add type selector
    {
        return null;
    }

    public void ReturnToPool(EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        // Return to pool
    }
}

// Enemy Wave data object
