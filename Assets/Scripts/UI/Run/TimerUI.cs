using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;

    [SerializeField] private TextMeshProUGUI timerText;

    private void Start()
    {
        cg.alpha = 0f;
    }

    private void OnEnable()
    {
        RunState.onTimerUpdated += HandleTimerUpdated;
    }

    private void OnDisable()
    {
        RunState.onTimerUpdated -= HandleTimerUpdated;
    }

    private void HandleTimerUpdated(float time)
    {

        if (time < 0) time = 0;

        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

}
