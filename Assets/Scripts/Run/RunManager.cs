using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    public GameDatabase db;

    [Header("Refs")]

    [SerializeField] private Player playerPrefab;

    [SerializeField] private Transform roomContainer;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform playerContainer;

    [SerializeField] private float fadeTime;
    [SerializeField] private CanvasGroup fadeOverlay;

    [SerializeField] private MapOverlayScreen mapDisplay;
    [SerializeField] private RunEndOverlay endOverlay;

    [Header("Run Config")]

    public RoomPoolService roomService; // Pools rooms, holds useful values etc
    public EnemyService enemyService; // Pools enemies, allows for spawning and handling etc

    public RunState currentRun;

    private Player activePlayer;
    private RoomManager activeRoom;


    public event Action<Vector2Int> onPlayerRoomChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        roomService = new RoomPoolService(db.rooms, roomContainer);
        enemyService = new EnemyService(db.enemies, enemyContainer);
    }

    private void OnEnable()
    {
        GameEvents.OnRunEnded += HandleRunEnd;
    }

    private void OnDestroy()
    {
        GameEvents.OnRunEnded -= HandleRunEnd;
    }

    private void Update()
    {
        UpdateRunTick();
    }

    #region Initializing Runs

    public void InitializeRun()
    {
        if (RunDataCarrier.IsNewRun)
        {
            BeginNewRun();
            return;
        }
        else
        {
            ResumeSavedRun();
            return;
        }
    }

    public void BeginNewRun()
    {
        RunConfig config = RunDataCarrier.ConsumeNewRunData();
        if (config == null)
        {
            Debug.LogError("[RunManager] No RunConfig exists");
            return;
        }

        activePlayer = Instantiate(playerPrefab, playerContainer);
        UIManager.Instance.Initialize(activePlayer);
        AIManager.Instance.Initialize(activePlayer);

        activePlayer.SetupNew(config.loadout);
        activePlayer.SetPlayerCanAct(true);

        currentRun = new(config.seed, config.map, activePlayer);
        roomService.BuildPool(config.map);

        EnterNode(config.map.startNode, null);
    }

    public void ResumeSavedRun()
    {
        //RunSaveData save = RunDataCarrier.ConsumeSavedRunData();
        //if (save == null)
        //{
        //    Debug.LogError("[RunManager] No RunSaveData exists");
        //    return;
        //}

        //currentRun = RunState.BuildFromSave(save);
        //roomService.BuildPool(currentRun.map);

        //activePlayer = Instantiate(playerPrefab, playerContainer);
        //activePlayer.SetupFromSave();

        //EnterNode(config.map.startNode, null);
    }

    #endregion

    private void HandleRunEnd(bool victory)
    {
        // Update profile based on run results

        var profile = GameManager.Instance.ActiveProfile;
        if (profile != null)
            RunSaveSystem.DeleteForSlot(profile.slotIndex);

        activePlayer.SetPlayerCanAct(false);

        endOverlay.Display(victory, currentRun);
    }

    #region Room Transitions

    private void EnterNode(RoomNode node, Direction? arrivingFrom)
    {
        // Remove Current Room

        if (activeRoom != null)
        {
            roomService.ReturnToPool(activeRoom);
            activeRoom = null;
        }

        currentRun.map.currentNode = node;

        // Get active room from pool
        if (roomService == null)
        {
            Debug.LogError("[RunManager] No RoomService existing :(");
            return;
        }
        
        activeRoom = roomService.GetRoom(node);
        activeRoom.Initialize(node, currentRun);

        activeRoom.OnCleared += HandleRoomCleared;
        activeRoom.OnFailure += HandleRoomFailed;

        // Setup Player
        Doorway entryDoor = null;
        if (arrivingFrom.HasValue)
        {
            entryDoor = activeRoom.GetDoorwayFor(arrivingFrom.Value.Opposite());
            entryDoor?.DisableUntilPlayerExit();
        }

        Vector2 spawnPos = arrivingFrom.HasValue
            ? entryDoor.entryPoint.position
            : activeRoom.defaultEntryPoint;

        GameEvents.PlayerTransitionTeleport(spawnPos);
        activePlayer.transform.position = spawnPos;

        activePlayer.PrepareForRoomChange();

        onPlayerRoomChanged?.Invoke(node.coordinates);

        activeRoom.Activate();
    }

    private void HandleGateActivated(RoomManager room)
    {
        // Sets up new area to move into
    }

    private void HandleRoomCleared(RoomManager room)
    {
        room.OnCleared -= HandleRoomCleared;
        room.OnFailure -= HandleRoomFailed;

        if (!currentRun.map.currentNode.cleared)
        {
            currentRun.roomsCleared++;
            currentRun.map.currentNode.cleared = true;
        }

        CheckFloorVictory();
        SaveCurrentRun();
    }

    private void HandleRoomFailed(RoomManager room)
    {
        // Fire Run End Event
    }

    private void CheckFloorVictory()
    {
        if (currentRun.roomsCleared == currentRun.map.totalNodes)
        {
            HandleRunEnd(true);
        }
        else
        {
            return;
        }
    }

    // Called by doorways when walked through to progress
    public void TransitionTo(RoomNode nextNode, Direction exitDirection)
    {
        StartCoroutine(TransitionRoutine(nextNode, exitDirection));
    }

    private IEnumerator TransitionRoutine(RoomNode nextNode, Direction dir)
    {
        DoTransitionFade(true);
        activePlayer.Movement.StartDoorwayMovement();


        yield return new WaitForSecondsRealtime(fadeTime);

        TimescaleManager.Instance.PauseGame(this);

        activePlayer.Movement.EndDoorwayMovement();
        EnterNode(nextNode, dir);

        TimescaleManager.Instance.UnpauseGame(this);
        DoTransitionFade(false);
    }

    private void DoTransitionFade(bool enable)
    {
        if (fadeOverlay == null) return;

        float fadeValue = enable ? 1f : 0f;

        fadeOverlay?.DOKill();
        fadeOverlay.DOFade(fadeValue, fadeTime).SetUpdate(true);
    }

    #endregion

    private void UpdateRunTick()
    {
        if (currentRun == null) return;
        if (TimescaleManager.IsPaused) return;

        if (activeRoom == null) return;

        switch (activeRoom.State)
        {
            case RoomState.Active:
                currentRun.Tick(Time.deltaTime);
                break;
            case RoomState.Idle:
                currentRun.Tick(Time.deltaTime / 2);
                break;

            default: 
                break;
        }

        //switch (state)
        //{
        //    case ActiveRunState.RoomActive:
        //        currentRun.Tick(Time.deltaTime);
        //        break;

        //    case ActiveRunState.RoomTransition:
        //        currentRun.Tick(Time.deltaTime / 2);
        //        break;

        //    case ActiveRunState.Draft:
        //    case ActiveRunState.RunComplete:
        //    case ActiveRunState.RunFailure:
        //        break;
        //}
    }

    #region Save System

    private void SaveCurrentRun()
    {
        return; // TODO 

        var profile = GameManager.Instance.ActiveProfile;
        if(profile == null) return;

        var save = new RunSaveData()
        {
            seed = currentRun.seed,
            player = activePlayer.PackPlayerState()
        };

        RunSaveSystem.Save(save, profile);
    }

    #endregion
}