using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    public static bool GodMode {  get; private set; }
    [SerializeField] private bool enableGodMode;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        GodMode = enableGodMode;
    }
}
