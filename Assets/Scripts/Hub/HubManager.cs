using System;
using System.Collections.Generic;
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

    [HideInInspector] public PlayerProfile activeProfile;
    public RunLoadout pendingLoadout;
    public RunMap pendingMap;
    public int pendingSeed {  get; private set; }

    public bool CanBeginRun => pendingMap != null && pendingLoadout != null && pendingSeed != 0;
    private bool loadingToRun = false;

    private readonly Dictionary<HubState, HubScreen> screens = new();
    private HubScreen activeScreen;

    // Services
    private MapGenerationService mapGeneration;

    public event Action<RunMap> onMapGenerated;

    [Header("Testing Values")]

    [SerializeField] private PlayerWeapon defaultWeapon;
    [SerializeField] private RoomData[] defaultRooms;
    [SerializeField] private int walkLength;
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

    }

    public void InitializeHub()
    {
        mapGeneration = new(db.rooms, mapBounds, walkLength); // Replace bounds

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

    public void GenerateNewMap()
    {
        int seed = Mathf.RoundToInt(UnityEngine.Random.Range(0, 100000));

        print(seed);

        pendingMap = mapGeneration.GenerateMapWithSeed(seed);
        onMapGenerated?.Invoke(pendingMap);

        pendingSeed = seed;

        seedText.text = seed.ToString();
        seedText.color = CanBeginRun ? Color.green : Color.red;
    }


    public RunMap BuildTestingMap()
    {
        RunMap map = new();
        
        if (defaultRooms.Length <= 0) print("[HubManager] no rooms lol");

        for (int i = 0; i < defaultRooms.Length; i++)
        {
            MapNode newNode = new MapNode()
            {
                room = defaultRooms[i],
                coordinates = new Vector2Int(0, i),
                // Setup Connections - refer to Aesthosis??
                cleared = false
            };

            map.tiles[newNode.coordinates] = newNode;
        }

        map.startNode = map.tiles[Vector2Int.zero];
        map.startNode.ConnectTo(map.tiles[new Vector2Int(0, 1)], Direction.East);


        return map;
    }

    public void BeginNewRunFromHub()
    {
        if (loadingToRun) return;

        SaveProfile();

        loadingToRun = true;
        RunDataCarrier.BuildNewRunData(pendingSeed, pendingLoadout, pendingMap);
        SceneLoader.Instance.LoadRun();
    }

    public void BeginSavedRunFromHub()
    {
        if (loadingToRun) return;

        SaveProfile();

        loadingToRun = true;
        SceneLoader.Instance.LoadRun();
    }


    public void ShowScreen(HubState type)
    {
        if(activeScreen != null)
            activeScreen.Close();

        activeScreen = screens[type];
        activeScreen.Open(this);

        // Run Event?
    }

    #region Profile Helpers

    private void LoadActiveProfile(int saveSlot)
    {
        activeProfile = null;
    }

    private void SaveProfile()
    {
        print("[HubManager] IMPLEMENT PROFILE SAVING");
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
