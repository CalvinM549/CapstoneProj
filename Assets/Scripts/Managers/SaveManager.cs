using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{

    private static readonly string saveLocation = Path.Combine(Application.persistentDataPath, "Saves");

    public static void Save(GameSaveData saveData)
    {
        if (!Directory.Exists(saveLocation))
            Directory.CreateDirectory(saveLocation);

        string filePath = GetSavePath(saveData.saveSlot);

        string json = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(filePath, json);

        Debug.Log($"Game Saved in save slot {saveData.saveSlot}");
    }

    public static GameSaveData Load(int slot)
    {
        string filePath = GetSavePath(slot);

        if (!File.Exists(filePath))
            return null;

        string json = File.ReadAllText(filePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

        Debug.Log($"Slot {slot} loaded successfully");

        return saveData;
    }


    public static bool SaveExists(int slot)
    {
        string filePath = GetSavePath(slot);
        return File.Exists(filePath);
    }

    private static string GetSavePath(int slot)
    {
        return Path.Combine(saveLocation, $"save_slot_{slot}");
    }


    [Serializable] 
    public class GameSaveData
    {
        public int saveSlot;
        
        // Run Save

        // Progression Save
        public ProgressionData progressionSave;
    }

    [Serializable]
    public class ProgressionData
    {

    }
}
