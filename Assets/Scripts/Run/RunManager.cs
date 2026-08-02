using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;



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

    public RoomPoolService roomService; // Pools rooms, holds useful values etc
    public EnemyService enemyService; // Pools enemies, allows for spawning and handling etc
    private RoomState state;

    private RunState currentRun;

    private Player activePlayer;
    private RoomManager activeRoom;

    [SerializeField] private Player playerPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        roomService = new RoomPoolService(db.rooms, roomContainer);
        enemyService = new EnemyService(db.enemies, enemyContainer);
    }

    private void Update()
    {
        UpdateRunTick();
    }

    public void BeginRun()
    {
        RunConfig config = RunDataCarrier.ConsumeData();
        if (config == null)
        {
            Debug.LogError("[RunManager] No RunConfig exists");
            return;
        }

        currentRun = new(config.seed, config.map);
        roomService.BuildPool(config.map);

        activePlayer = Instantiate(playerPrefab, playerContainer);
        activePlayer.SetupNew(config.loadout);

        EnterNode(config.map.startNode, null);
    }

    public void LoadToRun(RunState run)
    {
        currentRun = run;

        activePlayer = Instantiate(playerPrefab, Vector2.zero, Quaternion.identity);
        activePlayer.SetupFromSave(null); // REMOVE NULL

        //EnterNode()
    }

    private void ReturnToHub(bool victory)
    {
        // Update player profile based on run results
        // Save profile changes

        SceneLoader.Instance.LoadHub();
        // Transition back to hub
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

        activeRoom.Activate();
    }

    private void HandleRoomCleared(RoomManager room)
    {
        room.OnCleared -= HandleRoomCleared;
        room.OnFailure -= HandleRoomFailed;

        currentRun.roomsCleared++;
        currentRun.map.currentNode.cleared = true;

        state = RoomState.RoomTransition;
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

    #endregion
}