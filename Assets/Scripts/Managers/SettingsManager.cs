using System;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance {  get; private set; }

    [SerializeField] private AudioMixer mixer;
    private const string masterParam = "MasterVolume";
    private const string musicParam = "MusicVolume";
    private const string sfxParam = "SFXVolume";

    public GameSettings Current {  get; private set; }
    public event Action<GameSettings> OnSettingsChanged;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Current = SettingsSaveSystem.LoadOrDefault();
    }

    private void Start()
    {
        ApplyAll();
    }

    public void Apply(GameSettings settings)
    {
        Current = settings;
        ApplyAll();
        OnSettingsChanged?.Invoke(Current);
    }

    public void Save() => SettingsSaveSystem.Save(Current);

    private void ApplyAll()
    {
        SetVolume(masterParam, Current.masterVolume);
        SetVolume(musicParam, Current.musicVolume);
        SetVolume(sfxParam, Current.sfxVolume);

        if (Screen.fullScreen != Current.fullscreen)
            Screen.fullScreenMode = Current.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        QualitySettings.vSyncCount = Current.vsync ? 1 : 0;

        if (Current.qualityPreset >= 0
            && Current.qualityPreset < QualitySettings.names.Length
            && QualitySettings.GetQualityLevel() != Current.qualityPreset)
        {
            QualitySettings.SetQualityLevel(Current.qualityPreset, true);
        }
    }

     private void SetVolume(string param, float linear)
    {
        if (mixer == null) return;

        float db = linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        mixer.SetFloat(param, db);
    }
}
