using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.STP;



public class RunManager : MonoBehaviour
{
    private enum ActiveRunState
    {
        Draft,
        RoomActive,
        RoomTransition,
        RunComplete,
        RunFailure
    }

    public static RunManager Instance { get; private set; }

    public GameDatabase db;

    [Header("Refs")]

    [SerializeField] private Player playerPrefab;

    [SerializeField] private Transform roomContainer;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform playerContainer;

    [SerializeField] private float fadeTime;
    [SerializeField] private CanvasGroup fadeOverlay;

    [SerializeField] private MapOverlayUI mapDisplay;
    [SerializeField] private RunEndOverlay endOverlay;

    [Header("Run Config")]

    [SerializeField] private float optionalRouteCost = 5f;

    public RoomPoolService roomService; // Pools rooms, holds useful values etc
    public EnemyService enemyService; // Pools enemies, allows for spawning and handling etc
    private ActiveRunState state;

    private RunState currentRun;

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
        RunUIManager.Instance.Initialize(activePlayer);
        AIManager.Instance.Initialize(activePlayer);

        activePlayer.SetupNew(config.loadout);


        currentRun = new(config.seed, config.map, activePlayer);
        roomService.BuildPool(config.map);

        if(mapDisplay != null)
            mapDisplay.InitializeMapView(currentRun.map.tiles.Keys.ToList());

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

        state = ActiveRunState.RoomActive;
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
        Vector2 spawnPos = arrivingFrom.HasValue
            ? activeRoom.GetEntryPointFor(arrivingFrom.Value.Opposite())
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

        if (mapDisplay != null)
            mapDisplay.HandleRoomCleared(currentRun.map.currentNode.coordinates);

        state = ActiveRunState.RoomTransition;

        SaveCurrentRun();
    }

    private void HandleRoomFailed(RoomManager room)
    {
        state = ActiveRunState.RunFailure;

        // Fire Run End Event
    }

    // Called by doorways when walked through to progress
    public void TransitionTo(RoomNode nextNode, Direction exitDirection)
    {
        // Disable current room
        //DoTransitionFade(true); // Do Visual hiding

        StartCoroutine(TransitionRoutine(nextNode, exitDirection));

        //roomService.ReturnToPool(activeRoom);
        //activeRoom = null;
        //EnterNode(nextNode, null); // Change null to exit direction
    }

    private IEnumerator TransitionRoutine(RoomNode nextNode, Direction dir)
    {
        DoTransitionFade(true);
        TimescaleManager.Instance.PauseGame(this);
        yield return new WaitForSecondsRealtime(fadeTime);

        EnterNode(nextNode, null); // Change null to exit direction

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