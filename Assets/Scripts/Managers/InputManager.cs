using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = new GameObject("InputManager Singleton", typeof(InputManager)).GetComponent<InputManager>();
                instance.inputActions = new InputSystem_Actions();
                instance.inputActions.Enable();
            }

            return instance;
        }
    }

    public InputSystem_Actions inputActions;


    private void Awake()
    {
        //
    }

    private void OnDisable()
    {
        if (inputActions == null) return;

        inputActions.Disable();
        inputActions = null;
    }
}
