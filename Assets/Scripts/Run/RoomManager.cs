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

    [SerializeField] private Doorway[] doorways;
    public Vector2 fallbackEntryPoint;

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

    public void Initialize(MapNode node, RunState run)
    {
        Data = node.room;
        this.run = run;

        //// set doors
        //foreach (var door in doorways)
        //{
        //    if (node.connections.TryGetValue(door.direction, out var neighbor))
        //    {
        //        door.gameObject.SetActive(true);
        //        door.SetDestination(neighbor);
        //    }
        //    else
        //        door.gameObject.SetActive(false);
        //}

        for (int i = 0; i < doorways.Length; i++)
        {
            var currentDoor = doorways[i];

            if (node.connections2.Count >= i + 1)
            {
                var connectedRoom = node.connections2[i];
                print($"Connected {currentDoor.name} to {connectedRoom.id}");
                currentDoor.gameObject.SetActive(true);
                currentDoor.SetDestination(connectedRoom);
            }
            else
            {
                currentDoor.gameObject.SetActive(false);
            }
        }


        State = RoomState.Idle;

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
        return fallbackEntryPoint;
    }

    private void HandleEncounterCleared()
    {
        if (State != RoomState.Active) return;

        State = RoomState.Cleared;
        if(objectiveTracker != null)
            objectiveTracker.OnEncounterCleared -= HandleEncounterCleared;

        // get reward

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

    public void ResetForPool()
    {
        OnCleared = null;
        OnFailure = null;

        if (objectiveTracker != null)
            objectiveTracker.OnEncounterCleared -= HandleEncounterCleared;

        State = RoomState.Idle;
    }
}
