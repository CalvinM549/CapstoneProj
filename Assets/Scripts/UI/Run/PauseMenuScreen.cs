using UnityEngine;

public class PauseMenuScreen : UIScreen
{
    public void OnResumeClicked() => UIManager.Instance.CloseTop();

    public void OnSettingsClicked()
    {

    }

    public void OnQuitClicked() => GameManager.Instance.ExitGame();
}
