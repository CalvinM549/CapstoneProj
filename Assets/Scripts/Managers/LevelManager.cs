using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour // Depreciated
{
    private MenuInputReader inputs;

    [SerializeField] private CanvasGroup pauseMenuUI;
    [SerializeField] private CanvasGroup deathUI;
    [SerializeField] private CanvasGroup winUI;

    [SerializeField] private TextMeshProUGUI timer;
    private bool timerActive;
    private float currentTime; 

    [SerializeField] private TextMeshProUGUI enemyCounter;
    private int currentEnemyCount;
    private int maxEnemyCount;

    [SerializeField] private CanvasGroup WeaponSelectUI;
    [SerializeField] private PlayerWeapon missile;
    [SerializeField] private PlayerWeapon cannon;


    private void Start()
    {
        timerActive = true;

        var enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        maxEnemyCount = enemies.Length;
        currentEnemyCount = maxEnemyCount;
        enemyCounter.SetText($"{currentEnemyCount}/{maxEnemyCount} Enemies");

        pauseMenuUI.alpha = 0f;
        deathUI.alpha = 0f;
        winUI.alpha = 0f;

        pauseMenuUI.gameObject.SetActive(false);
        deathUI.gameObject.SetActive(false);
        winUI.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        inputs = InputManager.Instance.MenuInputs;

        inputs.ExitPressed += HandleExitPressed;

        GameEvents.OnPlayerDeath += HandlePlayerDeath;
        GameEvents.OnEnemyKilled += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        inputs.ExitPressed -= HandleExitPressed;

        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        GameEvents.OnEnemyKilled -= HandleEnemyDeath;
    }

    private void Update()
    {
        if (timerActive)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void HandleExitPressed()
    {
        if (winUI.gameObject.activeInHierarchy || deathUI.gameObject.activeInHierarchy)
            return;

        if (!TimescaleManager.IsPaused)
        {
            pauseMenuUI.gameObject.SetActive(true);
            pauseMenuUI.DOFade(1f, 0.5f).SetUpdate(true);
            TimescaleManager.Instance.PauseGame();
        }
        else
        {
            pauseMenuUI.gameObject.SetActive(false);
            pauseMenuUI.alpha = 0f;
            TimescaleManager.Instance.UnpauseGame();
        }
    }

    private void HandleEnemyDeath(EnemyController enemy)
    {
        currentEnemyCount--;
        enemyCounter.SetText($"{currentEnemyCount}/{maxEnemyCount} Enemies");

        enemyCounter.rectTransform?.DOKill();
        enemyCounter.rectTransform.DOPunchScale(Vector2.one * 0.2f, 0.5f);

        if (currentEnemyCount <= 0)
        {
            HandleLevelComplete();
        }
    }

    private void UpdateTimerDisplay()
    {
        TimeSpan time = TimeSpan.FromSeconds(currentTime);

        timer.text = string.Format("{0:00}:{1:00}:{2:000}", time.Minutes, time.Seconds, time.Milliseconds);
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(PlayerDeathRoutine());
        timerActive = false;
    }

    private IEnumerator PlayerDeathRoutine()
    {
        yield return new WaitForSeconds(1f);

        deathUI.gameObject.SetActive(true);
        deathUI.DOFade(1f, 2f);
    }

    private void HandleLevelComplete()
    {
        StartCoroutine(PlayerWinRoutine());
        timerActive = false;
    }

    private IEnumerator PlayerWinRoutine()
    {
        yield return new WaitForSeconds(1f);

        winUI.gameObject.SetActive(true);
        winUI.DOFade(1f, 2f);

        yield return new WaitForSeconds(2f);

        TimescaleManager.Instance.PauseGame();
    }

    public void ResetLevel()
    {
        SceneLoader.Instance.ReloadLevel();
    }

    public void LoadToMenu()
    {
        SceneLoader.Instance.LoadMainMenu();
    }

    public void SelectWeapon(int weapon)
    {
        PlayerWeapon selectedWeapon = null;

        if (weapon == 0)
            selectedWeapon = missile;
        else
            selectedWeapon = cannon;

        var obj = GameObject.FindAnyObjectByType<Player>().GetComponent<Player>();
        obj.Combat.EquipRangedWeapon(selectedWeapon);
    }
}
