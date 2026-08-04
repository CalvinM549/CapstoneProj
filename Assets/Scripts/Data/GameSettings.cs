using System;
using UnityEngine;

[Serializable]
public class GameSettings : MonoBehaviour
{
    public float gameVolume;
    public float sfxVolume;
    public float musicVolume;

    public int qualityPreset = 1;
    public bool fullscreen = true;

    public string inputBindingsJson;
}

[Serializable]
public class MetaState
{
    public int lastActiveSlot = -1;
}