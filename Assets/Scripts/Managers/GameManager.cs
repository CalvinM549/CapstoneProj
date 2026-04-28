using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject player;
    [SerializeField] private Canvas deathCanvas;

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
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
    }

    private void HandleEscPressed()
    {

    }

    private void HandlePlayerDeath()
    {
        player.SetActive(false);
        deathCanvas.gameObject.SetActive(true);
    }


    public void PauseGame()
    {

    }

    public Vector2 GetCardinal(Vector2 input)
    {
        // Convert given vector to 8 point cardinal direction (N, NE, E, SE, S, SW, W, NW) and return it as a normalized vector
        return Vector2.zero;
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
