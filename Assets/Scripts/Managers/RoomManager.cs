using System;
using UnityEngine;

public enum RoomState
{
    Spawning,
    Active,
    Cleared,
    Failed
}

public class RoomManager : MonoBehaviour
{
    public event Action<RoomManager> OnCleared;
    public event Action<RoomManager> OnFailure;

    public RoomData Data {  get; private set; }
    public RoomState State { get; private set; }

    private IObjectiveTracker objectiveTracker;

    [SerializeField] private UpgradeStation upgradeStation;
    [SerializeField] private Doorway[] doorways;
    public Vector2 defaultEntryPoint;

    private CurrentRun runData;

    private void Awake()
    {
        objectiveTracker = GetComponent<IObjectiveTracker>();
    }

    private void Update()
    {
        if (State == RoomState.Active)
            objectiveTracker.Tick(Time.deltaTime);
    }

    public void Initialize(RoomData room, CurrentRun run, bool completed = false)
    {
        runData = run;
        Data = room;

        if (completed)
        {

        }
        else
        {
            State = RoomState.Spawning;

            // Enemy Spawns

            foreach (var door in doorways)
                door.Lock();
        }
    }

    public void Activate()
    {
        State = RoomState.Active;
        objectiveTracker.OnEncounterCleared += HandleEncounterCleared;
    }

    public Vector2 GetEntryPointFor(Doorway door)
    {
        return Vector2.zero;
    }

    private void HandleEncounterCleared()
    {
        if (State != RoomState.Active) return;

        State = RoomState.Cleared;
        objectiveTracker.OnEncounterCleared -= HandleEncounterCleared;

        upgradeStation.Enable();

        foreach (var door in doorways)
            door.Unlock();

        OnCleared?.Invoke(this);
    }

    public void HandleEncounterFailure()
    {
        if (State == RoomState.Cleared) return;
        State = RoomState.Failed;
        OnFailure?.Invoke(this);
    }

    public void ResetForPool()
    {
        // Room Pooler reset
    }
}
