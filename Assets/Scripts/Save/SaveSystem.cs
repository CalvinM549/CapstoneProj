using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class JsonFileHelper
{
    private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

    public static bool Save<T>(T data, string path)
    {
        string tmp = path + ".tmp";
        string bak = path + ".bak";

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.WriteAllText(tmp, JsonUtility.ToJson(data, true), Utf8);

            if(File.Exists(path))
                File.Replace(tmp, path, bak);
            else
                File.Move(tmp, path);

            return true;
        }

        catch (Exception ex)
        {
            Debug.LogError($"[JsonFileHelper] Save failed for {path}: {ex.Message}");
            TryDelete(tmp);
            return false;

        }
    }

    public static bool TryLoad<T>(string path, out T data, bool obfuscate = false, string key = null) where T : class
    {
        if (TryRead(path, out data)) return true;

        string bak = path + ".bak";
        if (TryRead(bak, out data))
        {
            Debug.LogWarning($"[JsonFileHelper] {path} unreadable, restored from backup");

            try { File.Copy(bak, path, overwrite: true); }
            catch (Exception ex) { Debug.LogWarning($"[JsonFileHelper] Could not heal {path}: {ex.Message}"); }

            return true;
        }

        if (File.Exists(path))
        {
            Debug.LogError($"[JsonFileHelper] {path} and its backup are both unreadable");
            try { File.Copy(path, path + ".corrupt", overwrite: true); } catch { /* best effort */ }
        }

        data = null;
        return false;

    }

    public static bool TryRead<T>(string path, out T data) where T : class
    {
        data = null;
        if(!File.Exists(path)) return false;

        try
        {
            string json = File.ReadAllText(path, Utf8);
            if (string.IsNullOrEmpty(json)) return false;

            data = JsonUtility.FromJson<T>(json);
            return data != null;
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"[JsonFileHelper] Failed reading {path}: {ex.Message}");
            data = null;
            return false;
        }
    }

    public static bool Exists(string path) => File.Exists(path) || File.Exists(path + ".bak");

    public static void Delete(string path)
    {
        TryDelete(path);
        TryDelete(path + ".bak");
        TryDelete(path + ".tmp");
        TryDelete(path + ".corrupt");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (Exception ex) 
        { 
            Debug.LogWarning($"[JsonFileHelper] Could not delete {path}: {ex.Message}"); 
        }
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

    public static bool Save(RunSaveData data, PlayerProfile owner)
    {
        data.version = RunSaveData.CurrentVersion;
        data.ownerProfileId = owner.profileID;
        data.saveTimestamp = DateTime.UtcNow.ToString("o");
        return JsonFileHelper.Save(data, PathForSlot(owner.slotIndex));
    }

    public static bool TryLoad(PlayerProfile owner, out RunSaveData data)
    {
        if (!JsonFileHelper.TryLoad(PathForSlot(owner.slotIndex), out data))
            return false;
 
        if (data.ownerProfileId != owner.profileID)
        {
            Debug.LogWarning("[RunSaveSystem] Run save belongs to a different profile, ignoring");
            data = null;
            return false;
        }
 
        if (data.version > RunSaveData.CurrentVersion)
        {
            Debug.LogWarning($"[RunSaveSystem] Run save is from a newer build (v{data.version}), ignoring");
            data = null;
            return false;
        }
 
        if (data.map == null || string.IsNullOrEmpty(data.currentNodeId))
        {
            Debug.LogWarning("[RunSaveSystem] Run save is missing map data, ignoring");
            data = null;
            return false;
        }
 
        return true;

    }
    public static bool HasValidRun(PlayerProfile profile) => JsonFileHelper.Exists(PathForSlot(profile.slotIndex));
    public static void DeleteForSlot(int slot) => JsonFileHelper.Delete(PathForSlot(slot));
}

public static class ProfileSaveSystem
{
    private const int MaxSlots = 4;

    private static string FolderPath => Path.Combine(Application.persistentDataPath, "Profiles");
    private static string PathForSlot(int slot) => Path.Combine(FolderPath, $"profile_{slot}.json");

    public static bool Save(PlayerProfile profile) => JsonFileHelper.Save(profile, PathForSlot(profile.slotIndex));
    public static bool TryLoad(int slot, out PlayerProfile profile)
    {
        if (!JsonFileHelper.TryLoad(PathForSlot(slot), out profile))
            return false;

        profile.slotIndex = slot;
        profile.EnsureValid();
        return true;
    }


    public static bool SlotHasProfile(int slot) => JsonFileHelper.Exists(PathForSlot(slot));
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
    private static string FilePath => Path.Combine(Application.persistentDataPath, "settings.json");

    public static void Save(GameSettings save) => JsonFileHelper.Save(save, FilePath);
    public static GameSettings LoadOrDefault() => JsonFileHelper.TryLoad(FilePath, out GameSettings data) ? data : new GameSettings();
}

public static class MetaStateSaveSystem
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "meta_state.json");
    public static void Save(MetaState state) => JsonFileHelper.Save(state, FilePath);
    public static bool TryLoad(out MetaState state) => JsonFileHelper.TryLoad(FilePath, out state);
}