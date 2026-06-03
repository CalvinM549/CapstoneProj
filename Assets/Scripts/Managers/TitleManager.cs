using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    private InputSystem_Actions inputActions;

    [SerializeField] private CanvasGroup overlayBackground;
    [SerializeField] private GameObject controlsOverlay;
    [SerializeField] private GameObject exitOverlay;

    private bool controlsActive;
    private bool confirmExitActive;

    private void Start()
    {
        exitOverlay.SetActive(false);
        controlsOverlay.SetActive(false);

        confirmExitActive = false;
        controlsActive = false;

        overlayBackground.alpha = 0f;
        overlayBackground.blocksRaycasts = false;
    }

    //private void OnEnable()
    //{
    //    inputActions = InputManager.Instance.inputActions;

    //    inputActions.UI.Exit.performed += HandleExitInput;
    //}

    //private void OnDisable()
    //{
    //    inputActions.UI.Exit.performed -= HandleExitInput;
    //}

    private void HandleExitInput(InputAction.CallbackContext ctx)
    {
        if (controlsActive)
        {
            ToggleControlsOverlay(false); 
            return;
        }

        if (confirmExitActive)
        {
            ToggleExitOverlay(false);
            return;
        }

        // If nothing is active
        ToggleExitOverlay(true);
    }

    public void OnStartPress()
    {
        SceneLoader.Instance.LoadLevel();
    }

    public void OnQuitPress()
    {
        Application.Quit();
    }

    public void ToggleControlsOverlay(bool enable)
    {
        controlsActive = enable;
        controlsOverlay.SetActive(enable);
        ToggleOverlayBackground(enable);
    }

    public void ToggleExitOverlay(bool enable)
    {
        confirmExitActive = enable;
        exitOverlay.SetActive(enable);
        ToggleOverlayBackground(enable);
    }
    
    private void ToggleOverlayBackground(bool enable)
    {
        overlayBackground.DOFade(enable ? 0.95f : 0f, 0.5f);
        overlayBackground.blocksRaycasts = enable;
    }
}
