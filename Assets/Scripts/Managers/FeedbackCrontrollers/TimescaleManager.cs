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
        GameEvents.OnPlayerDeath += DeathStop;
        //Pause game event?
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        GameEvents.OnHitConfirmed -= HandleHitstop;
        GameEvents.OnPlayerDeath -= DeathStop;
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
        RequestTimeSlow(hit.hitstopTime);
    }

    private void DeathStop()
    {
        RequestTimeSlow(2.5f);
        Invoke(nameof(PauseGame), 2.5f);
    }

    public void RequestTimeSlow(float duration)
    {
        if (currentHitstopRoutine != null)
        {
            StopCoroutine(currentHitstopRoutine);
        }

        currentHitstopRoutine = StartCoroutine(TimeSlowRoutine(duration));
    }

    public void KillAllTimeSlows()
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
    }

    private IEnumerator TimeSlowRoutine(float duration)
    {
        hitstopTimeElapsed = 0f;

        Time.timeScale = hitstopTimeScale;

        while (hitstopTimeElapsed < duration && !IsPaused)
        {
            hitstopTimeElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if(!IsPaused)
            Time.timeScale = 1.0f;
    }

}
