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
        Gameplay
    }

    public static GameManager Instance;

    [SerializeField] private GameDatabase db;

    public GameState CurrentState => currentState;
    private GameState currentState;

    public PlayerProfile ActiveProfile { get; private set; }
    private MetaProgressionService metaProgression;

    [SerializeField] private LayerMask groundLayer;
    public LayerMask GroundLayer => groundLayer;
    [SerializeField] private LayerMask wallLayer;
    public LayerMask WallLayer => wallLayer;


    [SerializeField] private Texture2D combatCursor;

    private bool tutorialActive;


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
                MainMenuManager.Instance.InitializeMenu();
                break;

            case SceneLoader.HUB:
                currentState = GameState.Hub;
                SetCursorActive(true);
                HubManager.Instance.InitializeHub();
                break;

            case SceneLoader.RUN:
                currentState = GameState.Gameplay;
                SetCursorActive(false);
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
        print("[GameManager] IMPLEMENT PROFILE SAVING");
    }

    #endregion

    #region Run Utilities

    public void ExitGame()
    {

    }

    #endregion
}





