using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private InputSystem_Actions inputActions;

    [SerializeField] private LayerMask groundLayer;
    public LayerMask GroundLayer => groundLayer;
    [SerializeField] private LayerMask wallLayer;
    public LayerMask WallLayer => wallLayer;

    [SerializeField] private Player player;
    [SerializeField] private Canvas deathCanvas;
    [SerializeField] private Canvas tutorialCanvas;

    [SerializeField] private Texture2D combatCursor;

    private bool tutorialActive;

    public GameState CurrentState => currentState;
    private GameState currentState;

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
        }

        inputActions = InputManager.Instance.inputActions;

    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        SetCursor();

        if (!tutorialActive)
        {
            tutorialActive = true;
            tutorialCanvas.gameObject.SetActive(true);
            TimescaleManager.Instance.PauseGame();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
        inputActions.UI.Exit.performed += HandleEscPressed;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        inputActions.UI.Exit.performed -= HandleEscPressed;
    }

    private void HandleEscPressed(InputAction.CallbackContext ctx)
    {
        if (tutorialActive)
        {
            tutorialCanvas.gameObject.SetActive(false);
            tutorialActive = false;
            TimescaleManager.Instance.UnpauseGame();
            return;
        }

        if (!player.Health.IsAlive)
        {
            ResetGame();
        }
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(PlayerDeathRoutine());
    }

    private IEnumerator PlayerDeathRoutine()
    {
        yield return new WaitForSeconds(2f);

        player.gameObject.SetActive(false);
        deathCanvas.gameObject.SetActive(true);
    }


    public void PauseGame()
    {

    }

    private void ResetGame()
    {
        SceneManager.LoadScene("TestingScene");

        deathCanvas.gameObject.SetActive(false);

        if (!tutorialActive && !TimescaleManager.IsPaused)
        {
            tutorialActive = true;
            tutorialCanvas.gameObject.SetActive(true);
            TimescaleManager.Instance.PauseGame();
        }
    }


    private void SetCursor()
    {
        Cursor.SetCursor(combatCursor, Vector2.zero, CursorMode.Auto);
    }
}




public enum GameState
{
    MainMenu,
    Hub,
    Draft,
    InRun,
    Paused,
    GameOver
}
