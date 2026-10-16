using System;
using TMPro;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;

    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        cg.alpha = 0.5f;
        ApplySettings(SettingsManager.Instance != null ? SettingsManager.Instance.Current : null);
    }

    private void OnEnable()
    {
        RunState.onTimerUpdated += HandleTimerUpdated;

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged += ApplySettings;
    }

    private void OnDisable()
    {
        RunState.onTimerUpdated -= HandleTimerUpdated;

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnSettingsChanged -= ApplySettings;
    }

    private void ApplySettings(GameSettings settings)
    {
        if (settings == null) return;
        timerText.enabled = settings.showRunTimer;
    }

    private void HandleTimerUpdated(float timeValue)
    {
        TimeSpan time = TimeSpan.FromSeconds(timeValue);

        timerText.text = time.ToString(@"mm\:ss\.ff");
    }

}
