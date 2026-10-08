using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private RunSaveData pendingSave;
    public bool HasSavedRun => pendingSave != null;

    private RunLoadout pendingLoadout;
    private SectorMap pendingMap;
    private int pendingSeed;

    public event Action<int> OnPendingSeedChanged;
    public event Action<SectorMap> OnPendingMapChanged;
    public event Action<RunLoadout> OnPendingLoadoutChanged;

    [Header("Testing Values")]

    [SerializeField] private PlayerWeapon defaultWeapon;

    [SerializeField] private TextMeshProUGUI seedText;
    [SerializeField] private Button resumeButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (DebugManager.GodMode)
            defaultWeapon = DebugManager.GodWeapon;

        loadingToRun = false;
    }

    private void Start()
    {
        GenerateNewMap();
    }

    public void InitializeHub()
    {
        RefreshSavedRun();

        mapGeneration = new(db.rooms, db.roomTypes, config);

        pendingLoadout = BuildDefaultLoadout();

        AudioManager.Instance.PlayMusicTrack("MenuMusic01", true);
    }

    public void RefreshSavedRun()
    {
        var profile = GameManager.Instance.ActiveProfile;
        if (profile == null || !RunSaveSystem.TryLoad(profile, out pendingSave))
        {
            pendingSave = null;
            Debug.Log("No pending save found");
        }

        resumeButton.gameObject.SetActive(HasSavedRun);
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

        //pendingMap = mapGeneration.GenerateMapWithSeed(pendingSeed, 0);
        
        pendingMap = mapGeneration.GenerateWithSeed(pendingSeed);

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
        Debug.Log("Attempting to load saved run");

        if (loadingToRun) return;
        Debug.Log(1);
        if (!HasSavedRun) return;
        Debug.Log(2);

        GameManager.Instance.SaveActiveProfile();
        loadingToRun = true;
        RunDataCarrier.BuildSavedRun(pendingSave);
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
