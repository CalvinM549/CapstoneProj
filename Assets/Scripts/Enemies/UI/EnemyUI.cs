using DG.Tweening;
using System;
using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup cg;

    [SerializeField] private RadialEnemyHealthBar healthBar;
    [SerializeField] private EnemyTargetedUI targetUI;
    [SerializeField] private TextStatusUI statusUI;

    private EnemyController owner;

    [SerializeField] private bool alwaysVisible;

    [SerializeField] private float hideDelay;
    [SerializeField] private float hideDuration;
    [SerializeField] private float showDuration;

    private bool isVisible;
    private Tween visibleTween;

    private float hideTimer;

    private void Awake()
    {
        owner = GetComponentInParent<EnemyController>();
    }

    private void OnEnable()
    {
        owner.onLockChange += HandleLockChange;
    }

    private void OnDisable()
    {
        if(owner != null)
            owner.onLockChange -= HandleLockChange;
    }

    private void Update()
    {
        UpdateHideTimer();
    }

    public void Initialize()
    {
        cg.alpha = 0f;
        isVisible = false;

        healthBar.Initialize();
        targetUI.Initialize();
        statusUI.Initialize();
    }

    #region Show/Hide

    private void UpdateHideTimer()
    {
        if (alwaysVisible || !isVisible) return;
        if (owner.hasPlayerLock) return;

        if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
                HideUI();
        }
    }

    public void ShowUI()
    {
        isVisible = true;

        visibleTween?.Kill();
        visibleTween = DOTween.Sequence()
            .Join(cg.DOFade(1f, showDuration).SetEase(Ease.OutQuad));
    }

    public void HideUI()
    {
        isVisible = false;

        visibleTween?.Kill();
        visibleTween = DOTween.Sequence()
            .Join(cg.DOFade(0f, hideDuration).SetEase(Ease.OutQuad));
    }

    #endregion

    private void HandleLockChange(bool locked)
    {
        if (!isVisible)
            ShowUI();

        hideTimer = hideDelay;
    }
}
