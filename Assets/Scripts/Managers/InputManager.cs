using System;
using UnityEngine;
using UnityEngine.InputSystem;


public enum InputContext
{
    Uninitialized,
    Gameplay,
    UI,
    Cutscene
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions inputActions;

    public GameplayInputReader PlayerInputs {  get; private set; }
    public MenuInputReader MenuInputs { get; private set; }

    public InputContext CurrentContext { get; private set; } = InputContext.Uninitialized;

    public event Action<InputContext> ContextChanged;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new InputSystem_Actions();
        PlayerInputs = new GameplayInputReader(inputActions);
        MenuInputs = new MenuInputReader(inputActions);

        inputActions.Global.Enable();

        inputActions.Global.Exit.performed += HandleExitPressed;
        
        SetContext(InputContext.Gameplay);

        Debug.Log("[InputManager] Inputs Enabled");
    }

    private void OnDisable()
    {
        if (inputActions == null) return;

        PlayerInputs?.Unsubscribe();
        MenuInputs?.Unsubscribe();
        inputActions.Global.Exit.performed -= HandleExitPressed;

        inputActions.Disable();
        inputActions.Dispose();
        inputActions = null;
    }

    public void SetContext(InputContext newContext)
    {
        if(CurrentContext == newContext) return;

        Debug.Log($"[InputManager] Inputs switching from {CurrentContext} to {newContext}");
        CurrentContext = newContext;

        switch (newContext)
        {
            case InputContext.Gameplay:
                inputActions.Gameplay.Enable();
                inputActions.UI.Disable();
                break;

            case InputContext.UI:
                inputActions.UI.Enable();
                inputActions.Gameplay.Disable();
                break;

            case InputContext.Cutscene:
                inputActions.Gameplay.Disable();
                inputActions.UI.Disable();
                break;
        }

        ContextChanged?.Invoke(newContext);
    }

    private void HandleExitPressed(InputAction.CallbackContext ctx)
    {
        // Depending on game context, display settings vs exit menus etc

        SceneLoader.Instance.QuitGame();
    }

    public Vector2 GetMousePosition()
    {
        return inputActions.Global.PointerPosition.ReadValue<Vector2>();
    }
}