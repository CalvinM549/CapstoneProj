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
        DontDestroyOnLoad(gameObject);

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

public class BufferedInput
{
    public float Timestamp;

    public BufferedInput()
    {
        Timestamp = Time.time;
    }

    public bool isValid(float bufferWindow) => Time.time - Timestamp < bufferWindow;
}