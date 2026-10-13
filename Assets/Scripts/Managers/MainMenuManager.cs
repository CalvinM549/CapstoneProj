using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void InitializeMenu()
    {
        AudioManager.Instance.PlayMusicTrack("MenuMusic01", true);
        UIManager.Instance.OpenScreen("baseScreen");
    }
}
