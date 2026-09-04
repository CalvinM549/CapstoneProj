using System;
using UnityEngine;

public enum RoomState
{
    Idle,
    Active,
    Cleared,
    Failed
}

public class RoomManager : MonoBehaviour
{
    public RoomData Data {  get; private set; }
    public RoomState State { get; private set; }

    private IObjectiveTracker objectiveTracker;

    [SerializeField] private UpgradeStation upgradeStation;
    [SerializeField] private GateStation gateStation;
    [SerializeField] private RestStation restStation;

    [SerializeField] private Doorway[] doorways;
    public Vector2 defaultEntryPoint;

    [SerializeField] private Transform[] enemySpawnPoints;

    private RunState run;

    public event Action<RoomManager> OnCleared;
    public event Action<RoomManager> OnFailure;

    private void Awake()
    {
        objectiveTracker = GetComponent<IObjectiveTracker>();
    }

    private void Update()
    {
        if (State == RoomState.Active && objectiveTracker != null)
            objectiveTracker.Tick(Time.deltaTime);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
        {
            HandleEncounterCleared();
        }
#endif
    }

    public void Initialize(RoomNode node, RunState run)
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

        State = RoomState.Idle;

        if (upgradeStation != null)
            upgradeStation.OnActivated.AddListener(HandleUpgradeActivated);
 
        if (restStation != null)
            restStation.OnActivated.AddListener(HandleRestActivated);

        if(gateStation != null)
            gateStation.OnActivated.AddListener(HandleGateActivated);

        if (node.cleared || objectiveTracker == null)
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

        if (objectiveTracker != null)
            objectiveTracker.OnEncounterCleared += HandleEncounterCleared;
        else
            HandleEncounterCleared();
    }

    public Doorway GetDoorwayFor(Direction fromDirection)
    {
        foreach (var door in doorways)
        {
            if (door.direction == fromDirection && door.gameObject.activeSelf)
                return door;
        }

        print($"[RoomManager] No doorway matching direction {fromDirection}, using default instead");
        return null;

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
        if(objectiveTracker != null)
            objectiveTracker.OnEncounterCleared -= HandleEncounterCleared;

        if(upgradeStation != null) upgradeStation.Enable();

        if(restStation != null) restStation.Enable();

        if(gateStation != null) gateStation.Enable();

        foreach (var door in doorways)
            door.Unlock();

        OnCleared?.Invoke(this);
        GameEvents.RoomCompleted();
    }

    public void HandleEncounterFailure()
    {
        if (State == RoomState.Cleared) return;
        State = RoomState.Failed;
        OnFailure?.Invoke(this);
    }

    private void HandleRestActivated()
    {
        run.RestoreStability(100f, true);
        run.RestoreAmmo(5);
        run.RestoreStructure(3);
    }

    private void HandleUpgradeActivated()
    {

    }

    private void HandleGateActivated()
    {

    }

    public void ResetForPool()
    {
        OnCleared = null;
        OnFailure = null;

        if (objectiveTracker != null)
            objectiveTracker.OnEncounterCleared -= HandleEncounterCleared;

        State = RoomState.Idle;
    }
}
