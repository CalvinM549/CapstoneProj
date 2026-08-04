using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    public static TitleManager Instance { get; private set; }

    // Default UI
    [SerializeField] private CanvasGroup baseGroup;
    [SerializeField] private Button continueButton;

    // Profiles SubUI
    [SerializeField] private CanvasGroup profilesGroup;
    [SerializeField] private Button[] profileSlots;

    // Settings SubUI
    [SerializeField] private CanvasGroup settingsGroup;

    // Exit Confirmation? not sure if needed
    [SerializeField] private CanvasGroup exitGroup;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void InitializeMenu()
    {
        baseGroup.alpha = 1f;

        profilesGroup.gameObject.SetActive(false);
        settingsGroup.gameObject.SetActive(false);
        exitGroup.gameObject.SetActive(false);

        // ContinueButton
        if (!MetaStateSaveSystem.TryLoad(out var meta) 
            || meta.lastActiveSlot < 0 
            || !ProfileSaveSystem.TryLoad(meta.lastActiveSlot, out var profile))
        {
            continueButton.gameObject.SetActive(false);
        }
        else
        {
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => ContinueGame(profile));
        }
    }



    private void ContinueGame(PlayerProfile profile)
    {
        GameManager.Instance.SetActiveProfile(profile);

        if (RunSaveSystem.TryLoad(profile, out var run))
            SceneLoader.Instance.LoadRun();
        else
            SceneLoader.Instance.LoadHub();
    }

    public void ViewProfiles()
    {
        baseGroup.gameObject.SetActive(false);
        profilesGroup.gameObject.SetActive(true);

        var slots = ProfileSaveSystem.GetAllSlots();
        for (int i = 0; i < profileSlots.Length; i++)
        {
            int slot = i;
            var existing = slots[i];

            // Set button visuals
            profileSlots[i].onClick.RemoveAllListeners();
            profileSlots[i].onClick.AddListener(() => LoadProfile(slot, existing));
        }
    }

    private void LoadProfile(int slot, PlayerProfile existing)
    {
        // load into hub with correect profile slot
        if (existing == null)
        {
            // Show UI for setting profile's name
            string enteredName = "";
            var profile = new PlayerProfile(slot, enteredName);
            ProfileSaveSystem.Save(profile);
            EnterHubWithProfile(profile);
            return;
        }

        EnterHubWithProfile(existing);
    }

    private void EnterHubWithProfile(PlayerProfile profile)
    {
        GameManager.Instance.SetActiveProfile(profile);
        SceneLoader.Instance.LoadHub();
    }

    public void ViewSettings()
    {

    }

    public void QuitGame()
    {
        SceneLoader.Instance.QuitGame();
    }

}
