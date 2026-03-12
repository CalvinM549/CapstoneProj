using System.Collections;
using UnityEngine;

public class HitstopManager : MonoBehaviour
{
    private static HitstopManager Instance;

    private Coroutine currentHitstopRoutine;

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
        
    }

    private void OnDisable()
    {
        
    }

    public void TriggerHitstop(float duration)
    {
        if (currentHitstopRoutine != null)
        {
            StopCoroutine(currentHitstopRoutine);
        }

        currentHitstopRoutine = StartCoroutine(HitstopRoutine(duration));
    }

    public void CancelHitstop()
    {
        if (currentHitstopRoutine != null)
            StopCoroutine(currentHitstopRoutine);

        Time.timeScale = 1.0f;
    }

    private IEnumerator HitstopRoutine(float duration)
    {
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f;
    }
}
