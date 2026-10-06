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
    public RoomData Data {  get; private set; }
    public RoomState State { get; private set; }

    private IObjectiveTracker objectiveTracker;
    private StationBase[] roomStations;

    [SerializeField] private Doorway[] doorways;
    public int ExitCount => doorways.Length;
    public Vector2 EntryPoint => fallbackEntryPoint;

    [SerializeField] private Vector2 fallbackEntryPoint;

    [SerializeField] private SpawnPoint[] spawnPoints;

    private RunState run;
    private MapNode node;

    public event Action<RoomManager> OnCleared;
    public event Action<RoomManager> OnFailure;

    public event Action<RoomManager> OnStationUsed;

    private void Awake()
    {
        objectiveTracker = GetComponent<IObjectiveTracker>();
        
        roomStations = GetComponentsInChildren<StationBase>();         
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

    public void Initialize(MapNode node, RunState run, RunServices services)
    {
        Data = node.room;
        this.node = node;
        this.run = run;

        foreach (var roomObj in roomStations)
            roomObj.Setup(node, run, services);

        for (int i = 0; i < doorways.Length; i++)
        {
            var currentDoor = doorways[i];

            if (node.exits.Count >= i + 1)
            {
                var connectedRoom = node.exits[i];
                currentDoor.gameObject.SetActive(true);
                currentDoor.SetDestination(connectedRoom);
            }
            else
            {
                currentDoor.gameObject.SetActive(false);
            }
        }


        State = RoomState.Idle;

        if (objectiveTracker == null)
        {
            // Keep door in open state
        }
        else
        {
            // Enemy Spawns
            objectiveTracker.Setup(node, run, services);

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

    public Transform GetEnemySpawnPoint(SpawnTag tag)
    {
        if(spawnPoints == null || spawnPoints.Length == 0)
            return null;

        List<SpawnPoint> validPoints = spawnPoints.Where(s => s.Accepts(tag)).ToList();

        return validPoints[UnityEngine.Random.Range(0, validPoints.Count)].transform;
    }

    private void HandleEncounterCleared()
    {
        if (State != RoomState.Active) return;

        State = RoomState.Cleared;
        if(objectiveTracker != null)
            objectiveTracker.OnEncounterCleared -= HandleEncounterCleared;

        // get reward

        foreach (var door in doorways.Where(d => d.gameObject.activeInHierarchy))
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
