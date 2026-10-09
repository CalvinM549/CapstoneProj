using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    public const int CurrentVersion = 1;
    public int version = CurrentVersion;

    public int slotIndex;
    public string profileID;
    public string profileName;

    public int metaCurrency = 0;

    // Run History
    public float bestRunTime;
    public int runsAttempted;
    public int runVictoryCount;
    public int runFailureCount;
    
    // Unlocks
    public List<string> unlockedArchives = new();

    public List<string> unlockedTools = new();
    public List<string> unlockedWeapons = new();

    public List<SavedLoadoutPreset> savedPresets = new();

    // Story Flags
    public bool tutorialSeen;
    public bool skipCutscene;

    #region Creation

    public PlayerProfile() { }

    public PlayerProfile(int slot, string name)
    {
        slotIndex = slot;
        profileID = Guid.NewGuid().ToString();
        profileName = name;
    }

    #endregion

    public void EnsureValid()
    {
        unlockedArchives ??= new List<string>();
        unlockedTools ??= new List<string>();
        unlockedWeapons ??= new List<string>();

        if(string.IsNullOrEmpty(profileID))
            profileID = Guid.NewGuid().ToString();

        version = CurrentVersion;
    }

    public void RegisterRunResult(bool victory, float runTime)
    {
        runsAttempted++;

        if (victory)
        {
            runVictoryCount++;
            if(bestRunTime <= 0f ||  runTime < bestRunTime)
                bestRunTime = runTime;
        }
        else
        {
            runFailureCount++;
        }
    }
}

[Serializable]
public class SavedLoadoutPreset
{
    public string weaponID;
    public string toolID;
}
