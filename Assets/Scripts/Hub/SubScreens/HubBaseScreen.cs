using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HubBaseScreen : UIScreen
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private TypewriterBox statusLine;

    private GameDatabase db;

    public override void Initialize()
    {
        db = GameManager.Database;
    }

    protected override void OnBeforeOpen(object payload) => Refresh();
    protected override void OnFocusRegained() { }

    private void Refresh()
    {
        resumeButton.gameObject.SetActive(HubManager.Instance.HasSavedRun);

        var profile = GameManager.Instance.ActiveProfile;
        if (profile != null && statusLine != null)
            statusLine.SetText($"{profile.profileName} // RUNS {profile.runsAttempted} // CLEARS {profile.runVictoryCount} // SHARDS {profile.metaCurrency}");
    }

    public void OnContinueClicked()
    {
        SetFocused(false);
        HubManager.Instance.BeginSavedRunFromHub();
    }

    public void OnDeployClicked()
    {
        //UIManager.Instance.OpenScreen("runScreen");

        SetFocused(false);
        HubManager.Instance.BeginNewRunFromHub();
    }
    public void OnLoadoutClicked() => UIManager.Instance.OpenScreen("loadoutScreen");
    public void OnRecordsClicked() => UIManager.Instance.OpenScreen("recordsScreen");
    public void OnSettingsClicked() => UIManager.Instance.OpenScreen("settingsScreen");

    public void OnArchiveClicked()
    {
        UIManager.Instance.OpenScreen("archiveScreen", new ArchiveScreenPayload
        {
            AllEntries = db.archives.All,
            UnlockedIDs = GameManager.Instance.ActiveProfile.unlockedArchives
        });
    }

    public void OnDisconnectClicked()
    {
        UIManager.Instance.OpenScreen("confirmScreen", new ConfirmRequest(
            $"DISCONNECT FROM {GameManager.Instance.ActiveProfile.profileName}?",
            () =>
            {
                GameManager.Instance.SaveActiveProfile();
                SceneLoader.Instance.LoadMainMenu();
            },
            "DISCONNECT"));
    }

    // Escape on the root offers to disconnect instead of doing nothing
    public override void HandleCancel() => OnDisconnectClicked();
}
