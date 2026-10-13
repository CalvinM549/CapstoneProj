using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubManager : MonoBehaviour
{
    public static HubManager Instance;

    public GameDatabase db;
    [SerializeField] private MapGenerationConfig config;

    private bool loadingToRun = false;

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
    public event Action<bool> OnSavedRunChanged;

    public int PendingSeed => pendingSeed;
    public SectorMap PendingMap => pendingMap;
    public RunLoadout PendingLoadout => pendingLoadout;
    public bool CanBeginRun => pendingMap != null && pendingLoadout?.weapon != null;




    [Header("Testing Values")]

    [SerializeField] private PlayerWeapon defaultWeapon;

    [SerializeField] private TextMeshProUGUI seedText;
    [SerializeField] private Button resumeButton;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (DebugManager.GodMode)
            defaultWeapon = DebugManager.GodWeapon;

        loadingToRun = false;
    }

    public void InitializeHub()
    {
        CheckSavedRun();

        mapGeneration = new(db.rooms, db.roomTypes, config);

        pendingLoadout = BuildDefaultLoadout();
        GenerateNewMap();


        UIManager.Instance.OpenScreen("baseScreen");
    }

    public void CheckSavedRun()
    {
        var profile = GameManager.Instance.ActiveProfile;
        if (profile == null || !RunSaveSystem.TryLoad(profile, out pendingSave))
        {
            pendingSave = null;
            Debug.Log("No pending save found");
        }

        resumeButton.gameObject.SetActive(HasSavedRun);
    }

    //public void GenerateNewMap() => GenerateMap(UnityEngine.Random.Range(1, 100000));
    //public void GenerateMapFromSeed(string seed) => GenerateMap(ParseSeed(seed));
    //private void GenerateMap(int seed)
    //{
    //    RNGManager.Instance.InitRNG(seed);
        
    //    pendingSeed = seed;
    //    OnPendingSeedChanged?.Invoke(pendingSeed);

    //    pendingMap = mapGeneration.GenerateWithSeed(seed);
    //    OnPendingMapChanged?.Invoke(pendingMap);
    //}

    private static int ParseSeed(string input)
    {
        if (int.TryParse(input, out int seed) && seed != 0) return seed;
        
        return 0;
    }

    public void SetLoadoutWeapon(PlayerWeapon weapon)
    {
        pendingLoadout.weapon = weapon;
        OnPendingLoadoutChanged?.Invoke(pendingLoadout);
    }

    public void SetLoadoutTool(PlayerTool tool)
    {
        pendingLoadout.tool = tool;
        OnPendingLoadoutChanged?.Invoke(pendingLoadout);
    }

    public void AbandonSavedRun()
    {
        var profile = GameManager.Instance.ActiveProfile;
        if (profile != null) RunSaveSystem.DeleteForSlot(profile.slotIndex);

        pendingSave = null;
        OnSavedRunChanged?.Invoke(false);
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

        seedText.color = CanBeginRun ? Color.green : Color.red;
    }

    public void GenerateNewMap()
    {
        InitializeNewRNG();

        //pendingMap = mapGeneration.GenerateMapWithSeed(pendingSeed, 0);
        
        pendingMap = mapGeneration.GenerateWithSeed(pendingSeed);

        OnPendingMapChanged?.Invoke(pendingMap);

        seedText.color = CanBeginRun ? Color.green : Color.red;
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
        if (!HasSavedRun) return;

        GameManager.Instance.SaveActiveProfile();
        loadingToRun = true;
        RunDataCarrier.BuildSavedRun(pendingSave);
        SceneLoader.Instance.LoadRun();
    }

    #endregion


}
