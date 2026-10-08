using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance;

    public static bool GodMode => Instance.enableGodMode;
    [SerializeField] private bool enableGodMode;

    public static PlayerWeapon GodWeapon => Instance.godModeWeapon;
    [SerializeField] private PlayerWeapon godModeWeapon;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
