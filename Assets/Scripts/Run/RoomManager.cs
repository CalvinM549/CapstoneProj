using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum RoomState
{
    Idle,
    Active,
    Cleared,
    Failed
}

public class RoomManager : MonoBehaviour
{
    public RoomData Data => node.room;
    public RoomState State { get; private set; }

    private IObjectiveTracker objectiveTracker;
    private RoomVariation variation;

    private StationBase[] roomStations;

    [SerializeField] private Doorway entryDoor;
    [SerializeField] private Doorway[] exitDoors;

    public Doorway EntryDoor => entryDoor;
    public int ExitCount => exitDoors.Length;
    public Vector2 EntryPoint => fallbackEntryPoint;

    [SerializeField] private Vector2 fallbackEntryPoint;

    [SerializeField] private SpawnPoint[] spawnPoints;

    private RunState run;
    private MapNode node;

    public event Action<RoomManager> OnCleared;
    public event Action<RoomManager> OnFailure;

    public event Action<RoomManager> OnStationUsed;
    public event Action<Doorway> OnExitChosen;

    private void Awake()
    {
        objectiveTracker = GetComponent<IObjectiveTracker>();
        variation = GetComponent<RoomVariation>();

        roomStations = GetComponentsInChildren<StationBase>();
    }

    private void OnEnable()
    {
        foreach (var door in exitDoors)
            door.OnChosen += HandleDoorChosen;
    }

    private void OnDestroy()
    {
        foreach (var door in exitDoors)
            door.OnChosen -= HandleDoorChosen;
    }

    private void Update()
    {
        if (State == RoomState.Active && objectiveTracker != null)
            objectiveTracker.Tick(Time.deltaTime);
    }

    public void Initialize(MapNode node, RunState run, RunServices services)
    {
        this.node = node;
        this.run = run;

        variation?.Apply(run.Seed + node.depth);

        foreach (var roomObj in roomStations)
            roomObj.Setup(node, run, services);

        for (int i = 0; i < exitDoors.Length; i++)
        {
            var currentDoor = exitDoors[i];

            if (node.exits.Count >= i + 1)
            {
                var connectedRoom = node.exits[i];
                currentDoor.SetDestination(connectedRoom);
            }
            else
            {
                currentDoor.HideDoor();
            }
        }


        State = RoomState.Idle;

        if (objectiveTracker != null)
        {
            // Enemy Spawns
            objectiveTracker.Setup(node, run, services);

            foreach (var door in exitDoors)
                if(door.InUse) door.Lock();
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

    private void HandleDoorChosen(Doorway door) => OnExitChosen?.Invoke(door); 

    private void HandleEncounterCleared()
    {
        if (State != RoomState.Active) return;

        State = RoomState.Cleared;
        if(objectiveTracker != null)
            objectiveTracker.OnEncounterCleared -= HandleEncounterCleared;

        // get reward

        foreach (var door in exitDoors.Where(d => d.InUse))
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



    public Transform GetEnemySpawnPoint(SpawnTag tag)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        List<SpawnPoint> validPoints = spawnPoints.Where(s => s.Accepts(tag)).ToList();

        return validPoints[UnityEngine.Random.Range(0, validPoints.Count)].transform;
    }


    #region editor util

#if UNITY_EDITOR

    [ContextMenu("Populate SpawnPoints")]
    private void PopulateSpawnPoints()
    {
        spawnPoints = Array.Empty<SpawnPoint>();
        spawnPoints = GetComponentsInChildren<SpawnPoint>();

        Debug.Log($"{name} populated {spawnPoints.Length} spawn points");
    }

#endif

    #endregion
}
