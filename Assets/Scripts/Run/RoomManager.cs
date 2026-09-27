using System;
using System.Linq;
using UnityEngine;

public enum RoomState
{
    Idle,
    Active,
    Cleared,
    Failed
}

[Serializable]
public class EnemySpawnPoint
{
    public string groupTag;
    public Transform point;
}

public class RoomManager : MonoBehaviour
{
    public RoomData Data {  get; private set; }
    public RoomState State { get; private set; }

    private IObjectiveTracker objectiveTracker;

    [SerializeField] private Doorway[] doorways;
    public Vector2 fallbackEntryPoint;

    [SerializeField] private EnemySpawnPoint[] enemySpawnPoints;

    private RunState run;
    private MapNode node;

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
        this.node = node;
        this.run = run;

        foreach (var roomObj in GetComponentsInChildren<StationBase>())
            roomObj.Setup(node, run, OnCleared);

        for (int i = 0; i < doorways.Length; i++)
        {
            var currentDoor = doorways[i];

            if (node.connections2.Count >= i + 1)
            {
                var connectedRoom = node.connections2[i];
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

    private readonly System.Collections.Generic.List<Transform> _taggedMatchesBuffer = new();

    public Transform GetEnemySpawnPoint(string groupTag)
    {
        if(enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            return null;


        if (!string.IsNullOrEmpty(groupTag))
        {
            _taggedMatchesBuffer.Clear();
            foreach (var sp in enemySpawnPoints)
                if (sp.groupTag == groupTag)
                    _taggedMatchesBuffer.Add(sp.point);

            if (_taggedMatchesBuffer.Count > 0)
                return _taggedMatchesBuffer[UnityEngine.Random.Range(0, _taggedMatchesBuffer.Count)];
        }

        return enemySpawnPoints[UnityEngine.Random.Range(0, enemySpawnPoints.Length)].point;

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
}
