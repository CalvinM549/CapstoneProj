using UnityEngine;

public class RunSaveManager : MonoBehaviour
{
    private Player player;


    #region Public Functions

    public void SaveRun()
    {
        RunSaveData data = new();

        CapturePlayerState(data.player);
        CaptureRunState(data);

        SaveSystem.Save(data);
    }

    public bool LoadRun()
    {
        if (!SaveSystem.TryLoad(out RunSaveData data))
        {
            Debug.Log("[RunSaveManager] no save found");
            return false;
        }

        if (!ValidateSave(data))
            return false;
        
        RestoreRunState(data);
        RestorePlayerState(data.player);
        

        return true;
    }

    public void DeleteSave() => SaveSystem.DeleteSave();

    public bool HasSave() => SaveSystem.HasSave();

    #endregion

    #region Capture

    private void CapturePlayerState(PlayerStateSave save)
    {

    }

    private void CaptureRunState(RunSaveData save)
    {

    }

    #endregion

    #region Restore

    private void RestorePlayerState(PlayerStateSave save)
    {

    }

    private void RestoreRunState(RunSaveData save)
    {

    }

    #endregion

    private bool ValidateSave(RunSaveData data)
    {
        if (data.saveVersion == "1") return true;

        Debug.LogWarning($"[RunSaveDamage] Save version {data.saveVersion} is not supported, deleting save");

        DeleteSave();
        return false;
    }
}
