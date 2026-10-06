using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/NewEnemyData", order = 1)]
public class EnemyData : DatabaseEntry
{
    [Header("Config")]
    public string enemyName;

    public EnemyController prefab;
    public AIProfile aiProfile;

    [Space]

    public float baseHealth;
    public float moveSpeed;

    public float baseDamageMult = 1f;
    public float baseResistanceMult = 1f;

    [Space]

    [Min(1)] public float cost;
    [Min(0f)] public float weight;

    public int minChapter;
    public int maxChapter;

    public int baseCurrencyDrop;
    public SpawnTag spawnTag;

    public bool AvaliableAt(int depth) => depth >= minChapter && depth <= maxChapter;

}

[Serializable]
public class EnemyWave
{
    public readonly List<EnemyData> enemies = new();

    public float startDelay;
}