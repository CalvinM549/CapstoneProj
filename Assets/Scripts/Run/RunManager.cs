using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.STP;



public class RunManager : MonoBehaviour
{
    private enum RoomState
    {
        Draft,
        RoomActive,
        RoomTransition,
        RunComplete,
        RunFailure
    }

    public static RunManager Instance { get; private set; }

    public GameDatabase db;

    [SerializeField] private Transform roomContainer;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform playerContainer;

    [SerializeField] private float fadeTime;
    [SerializeField] private CanvasGroup fadeOverlay;

    [SerializeField] private MapDisplay mapDisplay;
    [SerializeField] private RunEndOverlay endOverlay;

    public RoomPoolService roomService; // Pools rooms, holds useful values etc
    public EnemyService enemyService; // Pools enemies, allows for spawning and handling etc
    private RoomState state;

    private RunState currentRun;

    private Player activePlayer;
    private RoomManager activeRoom;

    [SerializeField] private Player playerPrefab;

    public event Action<Vector2Int> onPlayerRoomChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        roomService = new RoomPoolService(db.rooms, roomContainer);
        enemyService = new EnemyService(db.enemies, enemyContainer);

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

        currentRun = new(config.seed, config.map);
        roomService.BuildPool(config.map);

        activePlayer = Instantiate(playerPrefab, playerContainer);
        activePlayer.SetupNew(config.loadout);

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

    private void EnterNode(MapNode node, Direction? arrivingFrom)
    {
        // Remove Current Room

        if (activeRoom != null)
        {
            roomService.ReturnToPool(activeRoom);
            activeRoom = null;
        }

        state = RoomState.RoomActive;
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

    private void HandleRoomCleared(RoomManager room)
    {
        room.OnCleared -= HandleRoomCleared;
        room.OnFailure -= HandleRoomFailed;

        currentRun.roomsCleared++;
        currentRun.map.currentNode.cleared = true;

        mapDisplay.HandleRoomCleared(currentRun.map.currentNode.coordinates);

        state = RoomState.RoomTransition;

        // Save game?
    }

    private void HandleRoomFailed(RoomManager room)
    {
        state = RoomState.RunFailure;

        // Fire Run End Event
    }

    // Called by doorways when walked through to progress
    public void TransitionTo(MapNode nextNode, Direction exitDirection)
    {
        // Disable current room
        //DoTransitionFade(true); // Do Visual hiding

        StartCoroutine(TransitionRoutine(nextNode, exitDirection));

        //roomService.ReturnToPool(activeRoom);
        //activeRoom = null;
        //EnterNode(nextNode, null); // Change null to exit direction
    }

    private IEnumerator TransitionRoutine(MapNode nextNode, Direction dir)
    {
        DoTransitionFade(true);
        TimescaleManager.Instance.PauseGame();
        yield return new WaitForSecondsRealtime(fadeTime);

        EnterNode(nextNode, null); // Change null to exit direction

        TimescaleManager.Instance.UnpauseGame();
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

        switch (state)
        {
            case RoomState.RoomActive:
                currentRun.Tick(Time.deltaTime);
                break;

            case RoomState.RoomTransition:
                currentRun.Tick(Time.deltaTime / 2);
                break;

            case RoomState.Draft:
            case RoomState.RunComplete:
            case RoomState.RunFailure:
                break;

        }
    }

    #region Save System

    private void SaveCurrentRun()
    {
        var profile = GameManager.Instance.ActiveProfile;
        if(profile == null) return;

        var save = new RunSaveData()
        {
            seed = currentRun.RunSeed,
            player = activePlayer.PackPlayerState()
        };

        RunSaveSystem.Save(save, profile);
    }

    #endregion
}