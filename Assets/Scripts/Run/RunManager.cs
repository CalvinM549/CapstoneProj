using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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


    public static RunState CurrentRun => Instance.currentRun;
    public RunState currentRun;

    public Player activePlayer;
    public PlayerUI activePlayerUI;
    public RoomManager activeRoom;

    private bool runEnded;

    private bool transitioning;

    public event Action<Vector2Int> OnPlayerRoomChanged;

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

        SetupPlayer();

        activePlayer.SetupNew(config.loadout);
        activePlayer.SetPlayerCanAct(true);

        currentRun = new(config.map, activePlayer);

        EnterNode(currentRun.map.entryNode);
    }

    private void SetupPlayer()
    {
        activePlayer = Instantiate(playerPrefab, playerContainer);
        AIManager.Instance.Initialize(activePlayer);

        activePlayerUI = Instantiate(playerUIPrefab, playerUIContainer);
        activePlayerUI.Initialize(activePlayer);
    }

    public void ResumeSavedRun()
    {
        RunSaveData save = RunDataCarrier.ConsumeSavedRunData();
        if (save == null)
        {
            Debug.LogError("[RunManager] No RunSaveData exists");
            return;
        }

        SectorMap map = save?.map.ToMap(db.rooms, db.roomTypes, save.seed, save.chapter);
        MapNode node = (map != null && !string.IsNullOrEmpty(save.currentNodeId))
            ? map.GetNode(save.currentNodeId)
            : null;

        if (node == null)
        {

            Debug.LogError("[RunManager] Saved run is missing or incompatible with current content");
            var p = GameManager.Instance.ActiveProfile;
            if (p != null) RunSaveSystem.DeleteForSlot(p.slotIndex);
            SceneLoader.Instance.LoadHub();
            return;
        }

        SetupPlayer();
        activePlayer.SetupFromSave(save.player);
        activePlayer.SetPlayerCanAct(true);

        currentRun = RunState.FromSave(save, map, activePlayer);

        EnterNode(node, resuming: true);
    }

    #endregion

    private void HandleRunEnd(bool victory)
    {
        if (runEnded) return;
        runEnded = true;
        transitioning = true;

        activePlayerUI.SetUIAlpha(0f);
        activePlayer.SetPlayerCanAct(false);
        activePlayer.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;


        // Update profile based on run results

        var profile = GameManager.Instance.ActiveProfile;
        if (profile != null)
        {
            RunSaveSystem.DeleteForSlot(profile.slotIndex);
            profile.RegisterRunResult(victory, currentRun.runDurationTimer);
            GameManager.Instance.SaveActiveProfile();
        }

        string screenTag = victory
            ? "victoryScreen"
            : "failureScreen";

        UIManager.Instance.OpenScreen(screenTag, currentRun);

        //runEndOverlay.Display(victory, currentRun);
    }

    #region Room Transitions

    private void ReleaseActiveRoom()
    {
        if(activeRoom == null) return;

        activeRoom.OnCleared -= HandleRoomCleared;
        activeRoom.OnFailure -= HandleRoomFailed;

        activeRoom.OnExitChosen -= HandleExitChosen;

        roomService.Release(activeRoom);
        activeRoom = null;
    }

    private void EnterNode(MapNode newNode, bool resuming = false)
    {
        // Remove Current Room
        runServices.currency.ForceCollectAll();
        ReleaseActiveRoom();

        if (!resuming) currentRun.EnterNode(newNode);

        activeRoom = roomService.Spawn(newNode.room);
        activeRoom.Initialize(newNode, currentRun, runServices);
        activeRoom.OnCleared += HandleRoomCleared;
        activeRoom.OnFailure += HandleRoomFailed;
        activeRoom.OnExitChosen += HandleExitChosen;

        // Setup Player
        GameEvents.PlayerTransitionTeleport(activeRoom.EntryPoint);
        activePlayer.transform.position = activeRoom.EntryPoint;

        activePlayer.PrepareForRoomChange();

        activePlayerUI.ToggleUI(true);

        OnPlayerRoomChanged?.Invoke(newNode.coordinates);

        SaveCurrentRun();
        activeRoom.Activate();
    }

    private void HandleRoomCleared(RoomManager room)
    {
        activePlayerUI.ToggleUI(false);

        if (currentRun.CurrentNode.type.isEnd) HandleRunEnd(true);
    }

    private void HandleRoomFailed(RoomManager room)
    {
        // Fire Run End Event
    }

    private void HandleExitChosen(Doorway door)
    {
        if (transitioning) return;

        StartCoroutine(TransitionRoutine(door.Destination, door.exitDirection));
    }

    private IEnumerator TransitionRoutine(MapNode nextNode, Direction dir)
    {
        transitioning = true;

        try
        {
            DoTransitionFade(true);
            activePlayer.Movement.StartDoorwayMovement(dir);

            yield return new WaitForSeconds(fadeTime);

            TimescaleManager.Instance.PauseGame(this);

            activePlayer.Movement.EndDoorwayMovement();
            EnterNode(nextNode);

            TimescaleManager.Instance.UnpauseGame(this);
            DoTransitionFade(false);
        }
        finally
        {
            transitioning = false;
        }
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

        if(runEnded) return;
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
        var profile = GameManager.Instance.ActiveProfile;
        if (profile == null || currentRun == null || runEnded)
        {
            Debug.Log("[RunManager] No Profile or No Run found");
            return;
        }

        if (!RunSaveSystem.Save(currentRun.ToSave(), profile))
            Debug.LogWarning("[RunManager] Run Save Failed");
        else
            Debug.Log("[RunManager] saved run");
    }

    #endregion
}