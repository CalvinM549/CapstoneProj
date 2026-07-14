using System.Collections;
using UnityEngine;

public class TimescaleManager : MonoBehaviour
{
    public static TimescaleManager Instance { get; private set; }

    public static bool IsPaused {  get; private set; }

    private Coroutine currentHitstopRoutine;

    private int pauseSources;
    private float lastTimeScale;

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
        GameEvents.OnHitConfirmed -= HandleHitstop;
        GameEvents.OnPlayerDeath -= DeathStop;
    }

    public void PauseGame()
    {
        pauseSources++;

        if (!IsPaused)
        {
            lastTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            IsPaused = true;
        }
    }

    public void UnpauseGame()
    {
        pauseSources--;

        if (pauseSources <= 0 && IsPaused)
        {
            Time.timeScale = lastTimeScale;
            IsPaused = false;
        }
    }

    private void HandleHitstop(HitData hit)
    {
        DoHitstop(hit.hitstopTime);
    }

    private void DeathStop()
    {
        DoHitstop(2.5f);
    }

    public void DoHitstop(float duration)
    {
        if (currentHitstopRoutine != null)
        {
            StopCoroutine(currentHitstopRoutine);
        }

        currentHitstopRoutine = StartCoroutine(HitstopRoutine(duration));
    }

    private IEnumerator HitstopRoutine(float duration)
    {
        Debug.Log("Hitstop Started");

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
