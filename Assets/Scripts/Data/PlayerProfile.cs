using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    public string profileName;
    public int profileSlot;

    public int metaCurrency;

    // Run History
    public float bestRunTime;
    public int runsAttempted;
    public int runVictoryCount;
    public int runFailureCount;

    // Meta buffs??
    public int BaseSegments;
    public float BaseTimer;
    
    // Unlocks
    public List<string> unlockedArchives;

    public List<string> unlockedTools;
    public List<string> unlockedWeapons;

    public List<SavedLoadoutPreset> savedPresets;
}

public class SavedLoadoutPreset
{

}
