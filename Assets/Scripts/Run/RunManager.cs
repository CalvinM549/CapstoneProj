using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class RunServices
{
    public EnemyService enemies;
    public EncounterService encounters;

    public LootService loot;
    public CurrencyDropService currency;
}

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    public GameDatabase db;
    [SerializeField] private EncounterGenerationConfig encounterConfig;

    [Header("Refs")]

    [SerializeField] private Player playerPrefab;
    [SerializeField] private PlayerUI playerUIPrefab;

    //[SerializeField] private DroneWeaponCarrier dronePrefab;

    [SerializeField] private Transform roomContainer;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private Transform playerUIContainer;

    [SerializeField] private Transform currencyContainer;
    [SerializeField] private CurrencyPickup currencyPickupPrefab;

    [SerializeField] private float fadeTime;
    [SerializeField] private CanvasGroup fadeOverlay;

    [SerializeField] private RunEndOverlay runEndOverlay;

    [Header("Run Config")]

    private RunServices runServices;

    public RoomPoolService roomService; // Pools rooms, holds useful values etc

    public RunState currentRun;

    public Player activePlayer;
    public PlayerUI activePlayerUI;
    public RoomManager activeRoom;

    public event Action<Vector2Int> onPlayerRoomChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        roomService = new RoomPoolService(roomContainer);

        runServices = new RunServices()
        {
            enemies = new EnemyService(db.enemies, enemyContainer),
            encounters = new EncounterService(db.enemies, encounterConfig),

            loot = new LootService(db),
            currency = new CurrencyDropService(currencyPickupPrefab, currencyContainer)
        };
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
        AIManager.Instance.Initialize(activePlayer);

        activePlayerUI = Instantiate(playerUIPrefab, playerUIContainer);
        activePlayerUI.Initialize(activePlayer);

        activePlayer.SetupNew(config.loadout);
        activePlayer.SetPlayerCanAct(true);

        currentRun = new(config.seed, config.map, activePlayer);

        EnterNode(currentRun.map.entryNode);
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
        activePlayer.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        GameManager.Instance.SetCursorActive(true);

        runEndOverlay.Display(victory, currentRun);
    }

    #region Room Transitions

    private void EnterNode(MapNode newNode)
    {
        // Remove Current Room
        runServices.currency.ForceCollectAll();

        if (activeRoom != null)
        {
            //roomService.ReturnToPool(activeRoom);
            //activeRoom = null;

            activeRoom.OnCleared -= HandleRoomCleared;
            roomService.Release(activeRoom);
        }

        // Setup room
        currentRun.EnterNode(newNode);
                
        activeRoom = roomService.Spawn(newNode.room);
        activeRoom.Initialize(newNode, currentRun, runServices);
        activeRoom.OnCleared += HandleRoomCleared;
        activeRoom.OnFailure += HandleRoomFailed;

        // Setup Player
        GameEvents.PlayerTransitionTeleport(activeRoom.EntryPoint);
        activePlayer.transform.position = activeRoom.EntryPoint;

        activePlayer.PrepareForRoomChange();

        activePlayerUI.ToggleUI(true);

        onPlayerRoomChanged?.Invoke(newNode.coordinates);

        activeRoom.Activate();
    }

    private void HandleRoomCleared(RoomManager room)
    {
        room.OnCleared -= HandleRoomCleared;
        room.OnFailure -= HandleRoomFailed;

        activePlayerUI.ToggleUI(false);
        CheckSectorVictory();
        SaveCurrentRun();
    }

    private void HandleRoomFailed(RoomManager room)
    {
        // Fire Run End Event
    }

    private void CheckSectorVictory()
    {
        if (currentRun.currentDepth == currentRun.map.rowCount)
        {
            HandleRunEnd(true); // temp for demo
        }
        else
        {
            return;
        }
    }

    // Called by doorways when walked through to progress
    public void TransitionTo(MapNode nextNode, Direction exitDirection)
    {
        StartCoroutine(TransitionRoutine(nextNode, exitDirection));
    }

    private IEnumerator TransitionRoutine(MapNode nextNode, Direction dir)
    {
        DoTransitionFade(true);
        activePlayer.Movement.StartDoorwayMovement(dir);


        yield return new WaitForSecondsRealtime(fadeTime);

        TimescaleManager.Instance.PauseGame(this);

        activePlayer.Movement.EndDoorwayMovement();
        EnterNode(nextNode);

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
    }

    #region Run Counters

    private void HandleEnemyDeath()
    {

    }

    #endregion

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