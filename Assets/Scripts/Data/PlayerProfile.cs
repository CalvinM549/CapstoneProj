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

    // Lifetime Stats
    public int totalEnemiesKilled;
    public int deepestDepth;
    public float totalTimeDeployed;

    // Unlocks
    public List<string> unlockedArchives = new();

    public List<string> unlockedTools = new();
    public List<string> unlockedWeapons = new();

    public List<SavedLoadoutPreset> savedPresets = new();
    public SavedLoadoutPreset lastLoadout = new();

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
        savedPresets ??= new List<SavedLoadoutPreset>();
        lastLoadout ??= new SavedLoadoutPreset();

        if(string.IsNullOrEmpty(profileID))
            profileID = Guid.NewGuid().ToString();

        version = CurrentVersion;
    }

    public void RegisterRunResult(bool victory, float runTime, int enemiesKilled = 0, int depthReached = 0)
    {
        runsAttempted++;

        totalTimeDeployed += Mathf.Max(0f, runTime);
        totalEnemiesKilled += Mathf.Max(0, enemiesKilled);
        deepestDepth = Mathf.Max(deepestDepth, depthReached);

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

public static class ProfileService
{
    public static bool TryGetContinueProfile(out PlayerProfile profile)
    {
        profile = null;
        return MetaStateSaveSystem.TryLoad(out var meta)
            && meta.lastActiveSlot >= 0
            && ProfileSaveSystem.TryLoad(meta.lastActiveSlot, out profile);
    }

    public static PlayerProfile CreateProfile(int slot)
    {
        var profile = new PlayerProfile(slot, $"UNIT_{slot + 1:00}");
        ProfileSaveSystem.Save(profile);
        return profile;
    }

    public static void DeleteProfile(int slot)
    {
        ProfileSaveSystem.DeleteSlot(slot);

        if (MetaStateSaveSystem.TryLoad(out var meta) && meta.lastActiveSlot == slot)
        {
            meta.lastActiveSlot = -1;
            MetaStateSaveSystem.Save(meta);
        }
    }

    public static void EnterProfile(PlayerProfile profile)
    {
        GameManager.Instance.SetActiveProfile(profile);

        var meta = MetaStateSaveSystem.TryLoad(out var existing) ? existing : new MetaState();
        meta.lastActiveSlot = profile.slotIndex;
        MetaStateSaveSystem.Save(meta);

        SceneLoader.Instance.LoadHub();
    }
}