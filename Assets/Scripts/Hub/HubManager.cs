using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum HubState
{
    Main,
    Draft,
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
    public int currentSeed {  get; private set; }

    public bool CanBeginRun => pendingMap != null && pendingLoadout != null && currentSeed != 0;
    private bool loadingToRun = false;

    private readonly Dictionary<HubState, HubScreen> screens;
    private HubScreen activeScreen;

    // Services
    private MetaProgressionService metaProgression;
    private MapGenerationService mapGeneration;

    [Header("Testing Values")]

    [SerializeField] private PlayerWeapon defaultWeapon;
    [SerializeField] private RoomData[] defaultRooms;

    [SerializeField] private TextMeshProUGUI seedText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Gather screens
        foreach (var screen in GetComponentsInChildren<HubScreen>(includeInactive: true))
            screens[screen.ScreenType] = screen;

        // Setup any instances
        // Load Profile from save system?
    }

    private void Start()
    {
        metaProgression = new(activeProfile);
        mapGeneration = new(db.rooms, 1002, new Vector2Int(5, 2)); // Replace bounds

        pendingLoadout = BuildDefaultLoadout();

        pendingMap = mapGeneration.GenerateMapWithSeed(1002); // TEMP FUNCTION
        //pendingMap = BuildTestingMap(); // TEMP FUNCTION
        currentSeed = 1002; // TEMP FUNCTION

        //ShowScreen(HubState.Main);


        seedText.text = currentSeed.ToString();
        seedText.color = CanBeginRun ? Color.green : Color.red;
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

    private RunMap BuildRunMap()
    {
        RunMap map = new()
        {

        };

        return map;
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

        //for (int i = 0; i < defaultRooms.Length; i++)
        //{
        //    print(i);
        //    var node = new MapNode()
        //    {
        //        room = defaultRooms[i],
        //        nodeIndex = i,
        //        row = 0,
        //        col = i,
        //        // Setup Connections - refer to Aesthosis??
        //        cleared = false
        //    };

        //    temp[i][0] = node;
        //}

        //RunMap map = new RunMap()
        //{

        //};

        //return map;
    }

    public void BeginNewRunFromHub()
    {
        if (loadingToRun) return;

        SaveProfile();

        loadingToRun = true;
        RunDataCarrier.BuildData(currentSeed, pendingLoadout, pendingMap);
        SceneLoader.Instance.LoadRun();
    }

    public void BeginSavedRunFromHub()
    {
        SaveProfile();

        SceneLoader.Instance.LoadRun();
    }


    public void ShowScreen(HubState type)
    {
        if(activeScreen != null)
            activeScreen.Close();

        activeScreen = screens[type];
        activeScreen.Open(this);

        // Run Event
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
