using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveFileName = "run_save.json";
    private const string BackupSuffix = ".bak";

    //Obfuscation
    private const bool UseObfuscation = false;
    private const string ObfuscationKey = "CINDERPATH_8766";

    //
    private static string SavePath = Path.Combine(Application.persistentDataPath, SaveFileName);
    private static string BackupPath => SavePath + BackupSuffix;

    //

    public static void Save(RunSaveData data)
    {
        data.saveTimestamp = DateTime.UtcNow.ToString("o");

        try
        {
            string json = JsonUtility.ToJson(data, true);
            string output = UseObfuscation ? Obfuscate(json) : json;

            if (File.Exists(SavePath))
                File.Copy(SavePath, BackupPath, overwrite: true);

            File.WriteAllText(SavePath, output, Encoding.UTF8);

            Debug.Log($"[SaveSystem] Run saved to {SavePath}");

        }

        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Save Failed: {ex.Message}");
        }
    }

    public static bool TryLoad(out RunSaveData data)
    {
        data = null;

        if(!File.Exists(SavePath))
            return false;

        try
        {
            data = LoadFromPath(SavePath);
            return data != null;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveSystem] Main Save corrupt ({ex.Message}), trying backup");
            return TryLoadBackup(out data);
        }
    }

    public static bool HasSave() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if(File.Exists(SavePath)) File.Delete(SavePath);
        if(File.Exists(BackupPath)) File.Delete(BackupPath);
        Debug.Log("[SaveSystem] Run Save Deleted");
    }

    #region Utilities

    private static RunSaveData LoadFromPath(string path)
    {
        string raw = File.ReadAllText(path, Encoding.UTF8);
        string json = UseObfuscation ? Deobfuscate(raw) : raw;
        var data = JsonUtility.FromJson<RunSaveData>(json);

        if (data == null)
            throw new Exception("JsonUtility returned null - file may be malformed");

        return data;
    }

    private static bool TryLoadBackup(out RunSaveData data)
    {
        data = null;

        if (!File.Exists(BackupPath))
        {
            Debug.LogError("[SaveSystem] No backup found");
            return false;
        }

        try
        {
            data = LoadFromPath(BackupPath);
            Debug.Log("[SaveSystem] Loaded from backup");
            return data != null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Backup save also corrupt: {ex.Message}");
            return false;
        }
    }

    // Obfuscation

    private static string Obfuscate(string input)
    {
        var key = ObfuscationKey;
        var sb = new StringBuilder(input.Length);

        for (int i = 0; i < input.Length; i++)
            sb.Append((char)(input[i] ^ key[i % key.Length]));

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string Deobfuscate(string input)
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(input));
        var key = ObfuscationKey;
        var sb = new StringBuilder(decoded.Length);

        for(int i = 0; i < decoded.Length; i++)
            sb.Append((char)(decoded[i] ^ key[i % key.Length]));

        return sb.ToString();
    }

    #endregion

}
