 using DG.Tweening;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Hub,
        Run
    }

    public static GameManager Instance;

    [SerializeField] private GameDatabase db;
    public static GameDatabase Database => Instance.db;

    [SerializeField] private MaterialDatabase materials;
    public static MaterialDatabase Materials => Instance.materials;

    public GameState CurrentState => currentState;
    private GameState currentState;

    public PlayerProfile ActiveProfile { get; private set; }
    public MetaProgressionService Meta => metaProgression;
    private MetaProgressionService metaProgression;

    [SerializeField] private LayerMask groundLayer;
    public static LayerMask GroundLayer => Instance.groundLayer;
    [SerializeField] private LayerMask wallLayer;
    public static LayerMask WallLayer => Instance.wallLayer;

    [SerializeField] private LayerMask enemyLayer;
    public static LayerMask EnemyLayer => Instance.enemyLayer;

    [SerializeField] private LayerMask playerLayer;
    public static LayerMask PlayerLayer => Instance.playerLayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_EDITOR
        SetActiveProfile(new PlayerProfile(0, "TESTPROFILE"));
        ActiveProfile.profileID = "TEST01";
#endif
    }

    private void OnEnable()
    {
        SceneLoader.Instance.onSceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.onSceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(string sceneName)
    {
        switch (sceneName)
        {
            case SceneLoader.MAINMENU:
                currentState = GameState.Menu;
                SetCursorActive(true);
                UIManager.Instance.Initialize();
                MainMenuManager.Instance.InitializeMenu();
                break;

            case SceneLoader.HUB:
                currentState = GameState.Hub;
                SetCursorActive(true);
                UIManager.Instance.Initialize();
                HubManager.Instance.InitializeHub();
                break;

            case SceneLoader.RUN:
                currentState = GameState.Run;
                SetCursorActive(false);
                UIManager.Instance.Initialize();
                RunManager.Instance.InitializeRun();
                break;
        }
    }

    public void SetCursorActive(bool active)
    {
        Cursor.visible = active;
    }

    #region Profile Utilities

    public void SetActiveProfile(PlayerProfile profile)
    {
        ActiveProfile = profile;
        metaProgression = new(profile, db);
    }

    public void SaveActiveProfile()
    {
        if (ActiveProfile == null)
        {
            Debug.Log("[RunManager] No Active Profile found");
            return;
        }

        if (!ProfileSaveSystem.Save(ActiveProfile))
            Debug.LogWarning("[GameManager] Profile Save Failed");
        else
            Debug.Log("[GameManager] saved profile");
    }

    #endregion

    #region Run Utilities

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion
}





