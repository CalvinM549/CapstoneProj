using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    public static TimescaleManager Instance { get; private set; }


    private List<object> pauseSources = new();
    public static bool IsPaused {  get; private set; }
    private float lastTimeScale;

    private Coroutine currentHitstopRoutine;
    private float hitstopTimeElapsed;

    private float hitstopEndUnscaled;


    [SerializeField] private float hitstopTimeScale = 0.05f;

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
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
        //Pause game event?
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        GameEvents.OnHitConfirmed -= HandleHitstop;
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
    }

    public void PauseGame(object source)
    {
        if (pauseSources.Contains(source)) return;

        pauseSources.Add(source);

        if (!IsPaused)
        {
            lastTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            IsPaused = true;
        }
    }

    public void UnpauseGame(object source)
    {
        pauseSources.Remove(source);

        if (pauseSources.Count == 0 && IsPaused)
        {
            Time.timeScale = lastTimeScale;
            IsPaused = false;
        }
    }

    public void ForceUnpause()
    {
        pauseSources.Clear();

        Time.timeScale = 1f;
        IsPaused = false;
    }

    private void HandleHitstop(HitData hit)
    {
        RequestHitStop(hit.hitstopTime);
    }

    private void HandlePlayerDeath()
    {
        RequestHitStop(2.5f);
    }

    public void RequestHitStop(float duration)
    {
        if(duration <= 0f) return;

        float end = Time.unscaledTime + duration;
        if (end <= hitstopEndUnscaled) return;

        hitstopEndUnscaled = end;

        if (currentHitstopRoutine == null)
            currentHitstopRoutine = StartCoroutine(HitstopRoutine());
    }

    public void KillAllHitstops()
    {
        if (currentHitstopRoutine != null)
        {
            StopCoroutine(currentHitstopRoutine);
            currentHitstopRoutine = null;

            if (!IsPaused)
            {
                Time.timeScale = 1.0f;
            }

            lastTimeScale = 1.0f;
        }

        hitstopEndUnscaled = 0f;
        currentHitstopRoutine = null;
    }

    private IEnumerator HitstopRoutine()
    {
        if (!IsPaused) Time.timeScale = hitstopTimeScale;

        while (Time.unscaledTime < hitstopEndUnscaled || IsPaused)
            yield return null;

        Time.timeScale = 1f;
        currentHitstopRoutine = null;
    }

}
