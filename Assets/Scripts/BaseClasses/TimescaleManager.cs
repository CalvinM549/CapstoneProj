using System.Collections;
using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    public static TimescaleManager Instance;

    public static bool IsPaused {  get; private set; }

    private Coroutine currentHitstopRoutine;

    private int pauseSources;
    private float lastTimeScale;

    private float hitstopTimeElapsed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnHitConfirmed += HandleHitstop;
        //Pause game event?
    }

    private void OnDisable()
    {
        GameEvents.OnHitConfirmed -= HandleHitstop;
    }

    public void PauseGame()
    {
        pauseSources++;

        if (!IsPaused)
        {
            lastTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void UnpauseGame()
    {
        pauseSources--;

        if (pauseSources == 0 && IsPaused)
        {
            Time.timeScale = lastTimeScale;
            IsPaused = false;
        }
    }

    public void DoHitstop(float duration)
    {
        if (currentHitstopRoutine != null)
        {
            StopCoroutine(currentHitstopRoutine);
        }

        currentHitstopRoutine = StartCoroutine(HitstopRoutine(duration));
    }

    private void HandleHitstop(HitData hit)
    {
        DoHitstop(hit.hitstopTime);
    }

    private IEnumerator HitstopRoutine(float duration)
    {
        Debug.Log("Hitstop Started");

        hitstopTimeElapsed = 0f;

        Time.timeScale = 0.05f;

        while (hitstopTimeElapsed < duration && !IsPaused)
        {
            hitstopTimeElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1.0f;
    }

}
