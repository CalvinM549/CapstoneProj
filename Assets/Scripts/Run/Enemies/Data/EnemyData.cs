using System;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemies/NewEnemyData", order = 1)]
public class EnemyData : DatabaseEntry
{
    [Header("Name")]
    public string enemyName;

    public EnemyController prefab;

    [Space]
    public float baseHealth;
    public float baseDamage;
    public float baseSpeed;

    [Header("AI")]
    public float detectionRadius;
}

[Serializable]
public class SpawnEntry
{
    public EnemyData enemyType;
    public int count;

    public string spawnGroupTag;
}

public class SpawnWave
{
    public SpawnEntry[] entries;

    public float startDelay;

    public int TotalCount()
    {
        int total = 0;
        foreach(var e in entries) 
            total += e.count;
        return total;
    }
}