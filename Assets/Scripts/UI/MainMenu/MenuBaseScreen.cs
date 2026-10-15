using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuBaseScreen : UIScreen
{
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueDetail;

    private PlayerProfile continueProfile;

    protected override void OnBeforeOpen(object payload) => RefreshContinue();
    protected override void OnFocusRegained() => RefreshContinue();

    private void RefreshContinue()
    {
        bool hasProfile = ProfileService.TryGetContinueProfile(out continueProfile);
        continueButton.interactable = hasProfile;

        if (continueButton != null)
        {
            continueDetail.text = !hasProfile
                ? "NO ACTIVE INSTANCE"
                : RunSaveSystem.HasValidRun(continueProfile)
                    ? $"{continueProfile.profileName} // RUN IN PROGRESS"
                    : continueProfile.profileName;
        }
    }

    public void OnContinueClicked()
    {
        if (continueProfile == null) return;
        SetFocused(false);
        ProfileService.EnterProfile(continueProfile);
    }

    public void OnInstancesClicked() => UIManager.Instance.OpenScreen("profilesScreen");

    public void OnSettingsClicked() => UIManager.Instance.OpenScreen("settingsScreen");

    public void OnExitClicked()
    {
        UIManager.Instance.OpenScreen("confirmScreen", new ConfirmRequest(
        "EXIT GAME?",
        () =>
        {
            GameManager.Instance.ExitGame(); ;
        },
        "EXIT"));
    }

    public override void HandleCancel()
    {
        OnExitClicked();
    }

}
