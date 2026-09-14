using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    public int slotIndex;
    public string profileID;
    public string profileName;

    public int metaCurrency = 0;

    // Run History
    public float bestRunTime;
    public int runsAttempted;
    public int runVictoryCount;
    public int runFailureCount;

    // Meta buffs??
    public int BaseSegments;
    public float BaseTimer;
    
    // Unlocks
    public HashSet<string> unlockedArchives;

    public HashSet<string> unlockedTools;
    public HashSet<string> unlockedWeapons;

    public List<SavedLoadoutPreset> savedPresets;

    // Story Flags
    public bool tutorialSeen;
    public bool skipCutscene;

    // New profile Builder
    public PlayerProfile(int slot, string name)
    {
        slotIndex = slot;
        profileID = Guid.NewGuid().ToString();
        profileName = name;

        runsAttempted = 0;

        unlockedArchives = new();
        unlockedTools = new();
        unlockedWeapons = new();

        tutorialSeen = false;
        skipCutscene = false;
    }
}

[Serializable]
public class SavedLoadoutPreset
{
    public string weaponID;
    public string toolID;
}
