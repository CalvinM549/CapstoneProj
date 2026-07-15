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

    private readonly Dictionary<HubState, HubScreen> screens;
    private HubScreen activeScreen;

    // Services
    private MetaProgressionService metaProgression;

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
    }

    private void Start()
    {
        // Load Profile from save system?
        pendingLoadout = BuildDefaultLoadout();
        
        pendingMap = BuildTestingMap(); // TEMP FUNCTION
        currentSeed = 1002; // TEMP FUNCTION

        //ShowScreen(HubState.Main);

        metaProgression = new(activeProfile);

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
        List<List<MapNode>> temp = new List<List<MapNode>>();
        int currentNodeIndex = 0;

        if (defaultRooms.Length <= 0) print("[HubManager] no rooms lol");

        for (int layerIndex = 0; layerIndex < defaultRooms.Length; layerIndex++)
        {
            List<MapNode> layer = new();

            // Loop through nodes in layer usually
            MapNode newNode = new MapNode()
            {
                room = defaultRooms[layerIndex],
                nodeIndex = currentNodeIndex,
                row = 0,
                col = layerIndex,
                // Setup Connections - refer to Aesthosis??
                cleared = false
            };
            layer.Add(newNode);
            currentNodeIndex++;

            temp.Add(layer);

        }

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

        RunMap map = new RunMap()
        {
            rows = temp,
            startNode = temp[0][0]
        };

        return map;
    }

    public void BeginNewRunFromHub()
    {
        SaveProfile();

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
