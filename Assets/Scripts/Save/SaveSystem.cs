using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class JsonFileHelper
{
    public static void Save<T>(T data, string path, bool obfuscate = false, string key = null)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            string output = obfuscate ? Obfuscation.Encode(json, key) : json;

            string backupPath = path + ".bak";
            if (File.Exists(path))
                File.Copy(path, backupPath, overwrite: true);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, output, Encoding.UTF8);

            Debug.Log($"[JsonFileHelper] File saved to {path}");
        }

        catch (Exception ex)
        {
            Debug.LogError($"[JsonFileHelper] Save failed for {path}: {ex.Message}");
        }
    }

    public static bool TryLoad<T>(string path, out T data, bool obfuscate = false, string key = null) where T : class
    {
        data = null;

        if (!File.Exists(path))
            return false;

        try
        {
            data = LoadFromPath<T>(path, obfuscate, key);
            return data != null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[JsonFileStore] {path} corrupt ({ex.Message}), trying backup");
            string backupPath = path + ".bak";
            if (!File.Exists(backupPath))
                return false;

            try 
            { 
                data = LoadFromPath<T>(backupPath, obfuscate, key); 
                return data != null; 
            }
            catch (Exception ex2)
            {
                Debug.LogError($"[JsonFileStore] Backup also corrupt: {ex2.Message}");
                return false;
            }
        }
    }

    private static T LoadFromPath<T>(string path, bool obfuscate, string key) where T : class
    {
        string raw = File.ReadAllText(path, Encoding.UTF8);
        string json = obfuscate ? Obfuscation.Decode(raw, key) : raw;
        var data = JsonUtility.FromJson<T>(json);
        if (data == null)
            throw new Exception("JsonUtil returned null, corrupted file");

        return data;
    }

    public static void Delete(string path)
    {
        if(File.Exists(path)) File.Delete(path);
        if (File.Exists(path + ".bak")) File.Delete(path + ".bak");
    }
}

public static class Obfuscation
{
    public static string Encode(string input, string key)
    {
        var sb = new StringBuilder(input.Length);
        for (int i = 0; i < input.Length; i++)
            sb.Append((char)(input[i] ^ key[i % key.Length]));

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public static string Decode(string input, string key)
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(input));
        var sb = new StringBuilder(decoded.Length);

        for (int i = 0; i < decoded.Length; i++)
            sb.Append((char)(decoded[i] ^ key[i % key.Length]));

        return sb.ToString();
    }
}

public static class RunSaveSystem
{
    private static string PathForSlot(int slot) => Path.Combine(Application.persistentDataPath, "Profiles", $"run_slot{slot}.json");

    public static void Save(RunSaveData data, PlayerProfile owner)
    {
        data.ownerProfileId = owner.profileID;
        data.saveTimestamp = DateTime.UtcNow.ToString("o");
        JsonFileHelper.Save(data, PathForSlot(owner.slotIndex));
    }

    public static bool TryLoad(PlayerProfile owner, out RunSaveData data)
    {
        data = null;
        if (!JsonFileHelper.TryLoad(PathForSlot(owner.slotIndex), out data))
            return false;

        if (data.ownerProfileId != owner.profileID)
        {
            Debug.LogWarning("[RunSaveSystem] Run save belongs to different owner, discarding");
            data = null;
            return false;
        }

        return true;
    }
    public static bool HasValidRun(PlayerProfile profile) => TryLoad(profile, out _);
    public static void DeleteForSlot(int slot) => JsonFileHelper.Delete(PathForSlot(slot));
}

public static class ProfileSaveSystem
{
    private const int MaxSlots = 4;

    private static string FolderPath => System.IO.Path.Combine(Application.persistentDataPath, "Profiles");
    private static string PathForSlot(int slot) => System.IO.Path.Combine(FolderPath, $"profile_{slot}.json");

    public static void Save(PlayerProfile profile) => JsonFileHelper.Save(profile, PathForSlot(profile.slotIndex));
    public static bool TryLoad(int slot, out PlayerProfile profile) => JsonFileHelper.TryLoad(PathForSlot(slot), out profile);

    public static bool SlotHasProfile(int slot) => File.Exists(PathForSlot(slot));
    public static void DeleteSlot(int slot)
    {
        JsonFileHelper.Delete(PathForSlot(slot));
        RunSaveSystem.DeleteForSlot(slot);
    }

    public static PlayerProfile[] GetAllSlots()
    {
        var result = new PlayerProfile[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
        {
            result[i] = TryLoad(i, out var p) ? p : null;
        }
        return result;
    }
}

public static class SettingsSaveSystem
{
    private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "settings.json");

    public static void Save(GameSettings save) => JsonFileHelper.Save(save, Path);
    public static GameSettings LoadOrDefault() => JsonFileHelper.TryLoad(Path, out GameSettings data) ? data : new GameSettings();
}

public static class MetaStateSaveSystem
{
    private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "meta_state.json");
    public static void Save(MetaState state) => JsonFileHelper.Save(state, Path);
    public static bool TryLoad(out MetaState state) => JsonFileHelper.TryLoad(Path, out state);
}