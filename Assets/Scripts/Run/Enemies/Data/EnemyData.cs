using System;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/NewEnemyData", order = 1)]
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
    public AIProfile aiProfile;
}

[Serializable]
public class WaveEntry
{
    public EnemyData enemyType;
    public int count;

    public string spawnGroupTag; // name of object that unit spawns at
}

public class SpawnWave
{
    public WaveEntry[] entries;

    public float startDelay;

    public int TotalEnemiesInWave()
    {
        int total = 0;
        foreach(var e in entries) 
            total += e.count;
        return total;
    }
}