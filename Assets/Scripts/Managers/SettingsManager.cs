using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance {  get; private set; }

    public float GameVolume {  get; private set; }
    public bool Fullscreen { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        LoadSavedSettings();
    }

    private void LoadSavedSettings()
    {

    }

    public void SaveGlobalVolume()
    {

    }
}
