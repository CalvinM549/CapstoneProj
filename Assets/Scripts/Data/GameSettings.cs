using System;
using UnityEngine;

[Serializable]
public class GameSettings
{
    public float masterVolume = 0.8f;
    public float sfxVolume = 0.7f;
    public float musicVolume = 0.8f;

    public bool vsync = true;
    public bool fullscreen = true;

    public int qualityPreset = -1;

    public string inputBindingsJson;
}

[Serializable]
public class MetaState
{
    public int lastActiveSlot = -1;
}