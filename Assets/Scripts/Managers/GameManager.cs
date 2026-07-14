using DG.Tweening;
using System.Collections;
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


    public GameState CurrentState => currentState;
    private GameState currentState;

    private InputSystem_Actions inputActions;

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
        }

    }

    public void SetState(GameState newState)
    {
        currentState = newState;
    }

    private void SetCursor()
    {
        Cursor.SetCursor(combatCursor, Vector2.zero, CursorMode.Auto);
    }
}





