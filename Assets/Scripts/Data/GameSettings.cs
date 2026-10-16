using System;
using UnityEngine;

[Serializable]
public class GameSettings
{
    // Audio
    public float masterVolume = 0.8f;
    public float sfxVolume = 0.7f;
    public float musicVolume = 0.8f;

    // Display
    public bool vsync = true;
    public bool fullscreen = true;

    public int qualityPreset = -1;

    // Gameplay
    [Range(0f, 1f)] public float screenShake = 1f;      // multiplier on CameraManager shake impulses
    [Range(0f, 1f)] public float glitchIntensity = 1f;  // multiplier on ScreenGlitch output
    public bool showRunTimer = true;

    public string inputBindingsJson;

    public GameSettings Clone() => (GameSettings)MemberwiseClone();
}

[Serializable]
public class MetaState
{
    public int lastActiveSlot = -1;
}