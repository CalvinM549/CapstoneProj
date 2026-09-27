using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;

    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        cg.alpha = 0.5f;
    }

    private void OnEnable()
    {
        RunState.onTimerUpdated += HandleTimerUpdated;
    }

    private void OnDisable()
    {
        RunState.onTimerUpdated -= HandleTimerUpdated;
    }

    private void HandleTimerUpdated(float timeValue)
    {
        TimeSpan time = TimeSpan.FromSeconds(timeValue);

        timerText.text = time.ToString(@"mm\:ss\.ff");
    }

}
