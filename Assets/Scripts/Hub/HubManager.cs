using System.Collections.Generic;
using UnityEngine;

public enum HubScreenType
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

    private readonly Dictionary<HubScreenType, HubScreen> screens;
    private HubScreen activeScreen;

    // Services
    private MetaProgressionService metaProgression;

    [SerializeField] private PlayerWeapon defaultWeapon;

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
        ShowScreen(HubScreenType.Main);

        metaProgression = new(activeProfile);
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
        RunMap map = new RunMap()
        {

        };

        return map;
    }

    public void BeginRunFromHub()
    {
        SaveProfile();

        SceneLoader.Instance.LoadRun();
    }


    public void ShowScreen(HubScreenType type)
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
        print("[MubManager] IMPLEMENT PROFILE SAVING");
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
