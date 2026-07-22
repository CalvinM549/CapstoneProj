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


    public GameState CurrentState => currentState;
    private GameState currentState;

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

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
    }

    private void SetCursor()
    {
        Cursor.SetCursor(combatCursor, Vector2.zero, CursorMode.Auto);
    }

    #region Profile Utilities



    #endregion

    #region Run Utilities

    private void BeginRunFromHub() // Called from hub
    {
        // Build data
        // Load to new scene
    }

    private void InitializeRun() // Called after run scene is initialized
    {
        // Begins a run with the correct information, i.e. saved run info vs built info

        RunManager.Instance.BeginRun();
    }

    #endregion
}





