using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public InputSystem_Actions inputActions;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        inputActions = new InputSystem_Actions();
        inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions == null) return;

        inputActions.Disable();
        inputActions = null;
    }
}
