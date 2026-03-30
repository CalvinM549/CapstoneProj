using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    public GameObject currentPlayer;

    public bool PlayerExists => currentPlayer != null;


    private void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {
        
    }

    #region Run Management

    public void StartRun(string firstRoom)
    {
        // generate seed
        // load to room drafter
    }

    #endregion

    #region Player Management

    public void SavePlayerState()
    {

    }

    #endregion
}

[Serializable]
public class RunData
{
    // Run
    public int RunSeed;

    // Player    
    PlayerData player;

    // Rooms
    public float HeatLevel;

    // Stats
    public int EnemiesKilled;
    public float RunDuration;
    public int DamageTaken;

}

[Serializable]
public class PlayerData
{
    public int CurrentHits;
    public int MaxHits;

    public float CurrentMomentum;

    public List<string> UpgradesGained = new();
}