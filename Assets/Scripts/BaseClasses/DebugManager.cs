using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    public static bool GodMode => Instance != null && Instance.enableGodMode;
    [SerializeField] private bool enableGodMode;

    public static PlayerWeapon GodWeapon => Instance != null ? Instance.godModeWeapon : null;
    [SerializeField] private PlayerWeapon godModeWeapon;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Lives in the bootstrap scene like the other managers, so it must survive scene loads
        DontDestroyOnLoad(gameObject);

#if !UNITY_EDITOR
        enableGodMode = false;
#endif
    }
}
