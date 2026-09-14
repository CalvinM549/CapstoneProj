using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum HubState
{
    Main,
    RunPrep,
    Loadout,
    Archive
}

public class HubManager : MonoBehaviour
{
    public static HubManager Instance;

    public GameDatabase db;
    [SerializeField] private MapGenerationConfig config;

    private bool loadingToRun = false;

    private readonly Dictionary<HubState, HubScreen> screens = new();
    private HubScreen activeScreen;

    // Services
    private MapGenerationService mapGeneration;

    private RunLoadout pendingLoadout;
    private SectorMap pendingMap;
    private int pendingSeed;

    public event Action<int> OnPendingSeedChanged;
    public event Action<SectorMap> OnPendingMapChanged;
    public event Action<RunLoadout> OnPendingLoadoutChanged;

    [Header("Testing Values")]

    [SerializeField] private PlayerWeapon defaultWeapon;
    [SerializeField] private RoomData[] defaultRooms;
    [SerializeField] private int walkLength;
    [SerializeField] private int walkerCount;
    [SerializeField] private Vector2Int mapBounds;

    [SerializeField] private TextMeshProUGUI seedText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Gather screens
        foreach (var screen in GetComponentsInChildren<HubScreen>(includeInactive: true))
        {
            screen.Initialize();
            screens[screen.ScreenType] = screen;
        }

        loadingToRun = false;
    }

    public void InitializeHub()
    {
        mapGeneration = new(db.rooms, config);

        pendingLoadout = BuildDefaultLoadout();

        GenerateNewMap();
    }

    private RunLoadout BuildDefaultLoadout()
    {
        RunLoadout loadout = new RunLoadout()
        {
            tool = null,
            weapon = defaultWeapon
        };

        return loadout;
    }

    public void ShowScreen(HubState type)
    {
        if (activeScreen != null)
            activeScreen.Close();

        activeScreen = screens[type];
        activeScreen.Open(this);

        // Run Event?
    }

    #region Run Generation

    public void InitializeNewRNG()
    {
        int seed = Mathf.RoundToInt(UnityEngine.Random.Range(0, 100000));
        RNGManager.Instance.InitRNG(seed);
        pendingSeed = seed;

        seedText.text = seed.ToString();

    }

    public void InitializeSetSeed(string inputSeed)
    {
        int seed = inputSeed.GetHashCode();
        RNGManager.Instance.InitRNG(seed);
        pendingSeed = seed;

        seedText.text = seed.ToString();

        seedText.color = CanBeginRun() ? Color.green : Color.red;
    }

    public void GenerateNewMap()
    {
        InitializeNewRNG();

        pendingMap = mapGeneration.GenerateMapWithSeed(pendingSeed, 0);
        OnPendingMapChanged?.Invoke(pendingMap);

        seedText.color = CanBeginRun() ? Color.green : Color.red;
    }

    private bool CanBeginRun()
    {
        return pendingMap != null && pendingLoadout != null && pendingSeed != 0;
    }

    public void BeginNewRunFromHub()
    {
        if (loadingToRun) return;

        GameManager.Instance.SaveActiveProfile();

        loadingToRun = true;
        RunDataCarrier.BuildNewRunData(pendingSeed, pendingLoadout, pendingMap);
        SceneLoader.Instance.LoadRun();
    }

    public void BeginSavedRunFromHub()
    {
        if (loadingToRun) return;

        GameManager.Instance.SaveActiveProfile();

        loadingToRun = true;
        SceneLoader.Instance.LoadRun();
    }

    #endregion


    #region LoadoutHelpers

    public void SetLoadoutSlot()
    {
        // metaprogresds slots
    }

    public void SetLoadoutWeapon(PlayerWeapon newWeapon)
    {
        pendingLoadout.weapon = newWeapon;
        // Ping event for loadout change
    }

    #endregion
}
