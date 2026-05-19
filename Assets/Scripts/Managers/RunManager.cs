using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum RunState
{
    Idle,
    Drafting,
    InRun,
    RunWin,
    RunLoss
}

public class RunManager : MonoBehaviour
{

    public static RunManager Instance { get; private set; }

    //public event Action<DraftedRoute> OnDraftReady;       // Draft generated, show UI
    //public event Action<RunState> OnRunStateChanged;  // Any state transition
    //public event Action<RoomManager> OnRoomStarted;      // New room is active
    //public event Action<RoomManager> OnRoomCleared;      // Room completed, doors open
    //public event Action<int, int> OnRoomProgressChanged; // (currentIndex, total)

    [SerializeField] private Transform roomSpawnPoint;

    public RunState State { get; private set; } = RunState.Idle;
    //Route
    
    private List<RoomConfig> orderedRooms;
    private int currentRoomIndex;
    private RoomManager activeRoomManager;
    private GameObject activeRoomGO;

    private void Awake()
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
        SetState(won ? RunState.RunWin : RunState.RunLoss);

        //Hook to things with event
    }

    private void SetState(RunState newState)
    {
        State = newState;
        //Fire Event
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