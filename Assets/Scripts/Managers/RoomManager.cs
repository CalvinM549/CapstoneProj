using System;
using System.Linq;
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

    private CurrentRun run;

    private void Awake()
    {
        objectiveTracker = GetComponent<IObjectiveTracker>();
    }

    private void Update()
    {
        if (State == RoomState.Active && objectiveTracker != null)
            objectiveTracker.Tick(Time.deltaTime);


        if (Input.GetKeyDown(KeyCode.L))
        {
            HandleEncounterCleared();
        }
    }

    public void Initialize(MapNode node, CurrentRun run)
    {
        Data = node.room;
        this.run = run;

        foreach (var door in doorways)
        {
            if (node.connections.TryGetValue(door.direction, out var neighbor))
            {
                door.gameObject.SetActive(true);
                door.SetDestination(neighbor);
            }
            else
                door.gameObject.SetActive(false);
        }

        State = RoomState.Spawning;

        if (node.cleared)
        {
            // Keep door in open state
        }
        else
        {
            // Enemy Spawns
            objectiveTracker.Setup(node.room, run);

            foreach (var door in doorways)
                if(door.gameObject.activeSelf) door.Lock();
        }
    }

    public void Activate()
    {
        State = RoomState.Active;
        objectiveTracker.OnEncounterCleared += HandleEncounterCleared;
    }

    public Vector2 GetEntryPointFor(Direction fromDirection)
    {
        foreach (var door in doorways)
        {
            if (door.direction == fromDirection && door.gameObject.activeSelf)
                return door.entryPoint.position;
        }

        print($"[RoomManager] No doorway matching direction {fromDirection}, using default instead");
        return defaultEntryPoint;
    }

    private void HandleEncounterCleared()
    {
        if (State != RoomState.Active) return;

        State = RoomState.Cleared;
        objectiveTracker.OnEncounterCleared -= HandleEncounterCleared;

        if(upgradeStation != null) // The case in rest / shop rooms?
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
