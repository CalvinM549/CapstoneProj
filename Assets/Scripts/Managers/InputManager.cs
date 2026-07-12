using System;
using UnityEngine;


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

    public InputReader PlayerInputs {  get; private set; }
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
        PlayerInputs = new InputReader(inputActions);
        MenuInputs = new MenuInputReader(inputActions);

        inputActions.Global.Enable();
        SetContext(InputContext.Gameplay);

        print("[InputManager] Inputs Enabled");
    }

    private void OnDisable()
    {
        if (inputActions == null) return;

        this.PlayerInputs?.Unsubscribe();
        this.MenuInputs?.Unsubscribe();

        inputActions.Disable();
        inputActions.Dispose();
        inputActions = null;
    }

    public void SetContext(InputContext context)
    {
        if(CurrentContext == context) return;

        CurrentContext = context;

        switch (context)
        {
            case InputContext.Gameplay:
                inputActions.Player.Enable();
                inputActions.UI.Disable();
                Debug.Log("[InputManager] Gameplay inputs enabled");
                break;

            case InputContext.UI:
                inputActions.UI.Enable();
                inputActions.Player.Disable();
                break;

            case InputContext.Cutscene:
                inputActions.Player.Disable();
                inputActions.UI.Disable();
                break;
        }

        ContextChanged?.Invoke(context);
    }


    public Vector2 GetMousePosition()
    {
        return inputActions.Player.PointerPosition.ReadValue<Vector2>();
    }
}