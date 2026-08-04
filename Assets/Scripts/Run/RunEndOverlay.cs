using TMPro;
using UnityEngine;

public class RunEndOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup overlay;

    [SerializeField] private CanvasGroup victoryOverlay;
    [SerializeField] private CanvasGroup failureOverlay;

    [Header("Stat Refs")]
    [SerializeField] private TextMeshProUGUI timerDisplay;
    [SerializeField] private TextMeshProUGUI RatingDisplay;
    [SerializeField] private TextMeshProUGUI enemiesKilledDisplay;
    [SerializeField] private TextMeshProUGUI damageTakenDisplay;

    private void Awake()
    {
        overlay.gameObject.SetActive(false);
    }

    public void Display(bool victory, RunState run)
    {
        // Add juice later
        overlay.gameObject.SetActive(true);

        victoryOverlay.gameObject.SetActive(victory);
        failureOverlay.gameObject.SetActive(!victory);

        //Stats
        timerDisplay.text = run.runDurationTimer.ToString();

        enemiesKilledDisplay.text = run.EnemiesKilled.ToString();
        damageTakenDisplay.text = run.DamageTaken.ToString();
    }

    public void ReturnToHub()
    {
        SceneLoader.Instance.LoadHub();
    }
}
