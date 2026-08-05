using DG.Tweening;
using System;
using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private bool AutoLoadMenu;

    [SerializeField] private float fadeTime;
    [SerializeField] private CanvasGroup fadeOverlay;

    public event Action<string> onSceneLoaded;

    public const string BOOTSTRAP = "BootstrapScene";
    public const string MAINMENU = "MenuScene";

    public const string HUB = "HubScene";
    public const string RUN = "RunScene";

    public const string LEVEL = "TestingScene";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        fadeOverlay.alpha = 0f;

#if UNITY_EDITOR
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            UnityEngine.SceneManagement.Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != BOOTSTRAP && scene.isLoaded)
            {
                SceneManager.SetActiveScene(scene);
                onSceneLoaded?.Invoke(scene.name);
                return;
            }
        }

#else
        LoadSceneImmediate(MAINMENU);
#endif
    }

    public void LoadMainMenu() => LoadScene(MAINMENU);
    public void LoadHub() => LoadScene(HUB);
    public void LoadRun() => LoadScene(RUN);

    public void LoadLevel() => LoadScene(LEVEL);
    public void ReloadLevel() => LoadScene(LEVEL);
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    #region Utilities

    private void LoadSceneImmediate(string sceneName)
    {
        var async = SceneManager.LoadSceneAsync(sceneName);
    }

    private void LoadScene(string sceneName, Action onLoad = null)
    {
        if (fadeOverlay != null)
        {
            fadeOverlay?.DOKill();
            fadeOverlay.DOFade(1f, fadeTime).SetUpdate(true);
        }

        StartCoroutine(LoadSceneRoutine(sceneName, onLoad));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, Action onLoad)
    {
        yield return new WaitForSecondsRealtime(fadeTime);

        var async = SceneManager.LoadSceneAsync(sceneName);

        while(!async.isDone)
            yield return null;

        onSceneLoaded?.Invoke(sceneName);
        onLoad?.Invoke();

        TimescaleManager.Instance.KillAllTimeSlows();

        if(TimescaleManager.IsPaused)
            TimescaleManager.Instance.UnpauseGame();

        if (fadeOverlay != null)
        {
            fadeOverlay?.DOKill();
            fadeOverlay.DOFade(0f, fadeTime).SetUpdate(true); ;
        }
    }

    #endregion
}
