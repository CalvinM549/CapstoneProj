using UnityEngine;

public class HubBaseScreen : UIScreen
{
    [SerializeField] private GameDatabase db;

    public void OnArchiveClicked()
    {
        var data = new ArchiveScreenPayload()
        {
            AllEntries = db.archives.All,
            UnlockedIDs = GameManager.Instance.ActiveProfile.unlockedArchives
        };
        UIManager.Instance.OpenScreen("archiveScreen", data);
    }

    public void OnRunHistoryClicked()
    {
        // open history
    }

    public void OnSettingsClicked()
    {

    }

    public void OnStartRunClicked()
    {

    }

    public override void HandleCancel()
    {
        // do nothing on esc
    }
}
