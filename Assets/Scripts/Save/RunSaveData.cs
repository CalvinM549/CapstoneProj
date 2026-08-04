using System;
using System.Collections.Generic;

[Serializable]
public class RunSaveData
{
    public string ownerProfileId;
    public string saveTimestamp = "";

    public int seed;
    public RunLoadout loadout;
    
    // Map
    public int currentNodeId;
    public int roomsCleared;

    // Player
    public float heatValue;

    // Stats
    public float runDuration;

    public PlayerStateSave player = new();
}

[Serializable]
public class PlayerStateSave
{
    public int activeSegmentIndex = 0;
    public List<SegmentSave> segments = new();

    public float currentMomentum = 0f;

    public string equippedTool = "";
    public string equippedWeapon = "";

    public PlayerUpgradeSave upgradeSave;
}

[Serializable]
public class SegmentSave
{
    public float currentHealth;
    public float maxHealth;
    public bool isActive;
    public bool isDestroyed;
}

[Serializable]
public class PlayerUpgradeSave
{
    // Each slot stores the ID of the equipped Perk
    public string majorFrame = "";
    public string majorWeapons = "";
    public string majorPropulsion = "";

    public List<string> subUpgradeIDs;

    public List<string> auxUpgradeIDs;
}