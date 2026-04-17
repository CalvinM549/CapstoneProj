using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    public GameObject currentPlayer;

    public RunData currentRun;

    public bool PlayerExists => currentPlayer != null;

    public bool RunActive;



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

    private void Update()
    {
        UpdateTimer();
    }

    #region Run Management

    public void StartRun(string firstRoom)
    {
        // generate seed
        // load to room drafter
    }

    public void EndRun(bool won)
    {
        // Display 
    }

    #endregion

    #region Player Management

    public void SavePlayerState()
    {

    }

    #endregion

    #region Timer Management

    public void UpdateTimer()
    {
        if (RunActive)
        {
            currentRun.RunDuration += Time.deltaTime;
        }
    }

    #endregion
}

[Serializable]
public class RunData
{
    // Run
    public int RunSeed;
    public float runTimer;

    // Player    
    public PlayerData player;

    // Rooms
    public float HeatLevel;

    // Stats
    public int EnemiesKilled;
    public float RunDuration;
    public int DamageTaken;

}


// Packed and saved after each room. Used to restore player when loading back into run.
[Serializable]
public class PlayerData
{
    // Structure info

    // Momentum info
    public float CurrentMomentum;

    // Upgrade info
    public List<string> UpgradesGained = new();
}