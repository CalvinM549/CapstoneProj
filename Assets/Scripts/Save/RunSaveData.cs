using System;
using System.Collections.Generic;

[Serializable]
public class RunSaveData
{
    public string saveVersion = "1";
    public string saveTimestamp = "";

    public UpgradeSaveData upgrades = new();
    public PlayerStateSave player = new();
    public RunProgressSave run = new();
}

[Serializable]
public class UpgradeSaveData
{
    // Each slot stores the ID of the equipped Perk
    public string majorFrame = "";
    public string majorWeapons = "";
    public string majorPropulsion = "";

    public List<string> subUpgradeIDs;

    public List<string> auxUpgradeIDs;
}

[Serializable]
public class PlayerStateSave
{
    public int activeSegmentIndex = 0;
    public List<SegmentSave> segments = new();

    public float currentMomentum = 0f;
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
public class RunProgressSave
{
    public int currentRoom;
    public int currentFloor;
    public int seed;
    public float runTimeSeconds;

}