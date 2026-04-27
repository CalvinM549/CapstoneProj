using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class HeatTimerManager : MonoBehaviour
{
    [SerializeField] Image image;

    [SerializeField]
    private float timeLimit;

    [SerializeField][Range(0, 1)]
    private float warningThreshold;

    [SerializeField][Range(0, 1)]
    private float criticalThreshold;

    private float elapsed;

    private bool isRunning;
    private bool hasExpired;
    private bool warningSent;
    private bool criticalSent;

    public float TimeRemaining => Mathf.Max(0f, timeLimit - elapsed);
    public float Normalized => Mathf.Clamp01(TimeRemaining / timeLimit);

    private void Start()
    {
        image.color = Color.white;

        isRunning = true;
    }

    void Update()
    {
        UpdateTimer();

        image.fillAmount = Normalized;
    }

    void UpdateTimer()
    {
        if (!isRunning) return;

        elapsed += Time.deltaTime;

        float remaining = TimeRemaining;
        float normalized = Normalized;

        if (!warningSent && normalized <= warningThreshold)
        {
            image.color = Color.orange;
            warningSent = true;
        }

        if (!criticalSent && normalized <= criticalThreshold)
        {
            image.color = Color.red;
            criticalSent = true;
        }

        if (!hasExpired && elapsed >= timeLimit)
        {
            hasExpired = true;
        }
    }
}
