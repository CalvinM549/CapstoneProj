using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class RadialEnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Image delayFillImage;

    [SerializeField] private RectTransform barRoot;

    [Header("Config")]

    [SerializeField] private bool alwaysVisible = false;
    [SerializeField] private float circleScale = 1f;

    [SerializeField] private float mainBarDuration;

    [SerializeField] private float hideDelay;
    [SerializeField] private float hideDuration;
    [SerializeField] private float showDuration;

    [SerializeField] private float delayFillDelay;
    [SerializeField] private float delayFillDuration;


    [SerializeField] private float hitPunchScale;
    [SerializeField] private float hitPunchDuration;

    private EnemyBase owner;
    private CanvasGroup canvasGroup;

    private float targetFill = 1f;
    private float hideTimer;
    private float delayFillTimer;

    private bool isVisible;
    private bool delayDrainPending;

    private Tween fillTween;
    private Tween delayFillTween;
    private Tween visibleTween;
    private Tween punchTween;

    private void Awake()
    {
        owner = GetComponentInParent<EnemyBase>();

        canvasGroup = barRoot.GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        barRoot.localScale = Vector3.one * 0.8f;
        isVisible = false;

        fillImage.rectTransform.localScale *= circleScale;
        delayFillImage.rectTransform.localScale *= circleScale;

        if (alwaysVisible)
            ShowBar();
    }


    private void OnEnable()
    {
        owner.OnHit += HandleHit;
        GameEvents.OnLockAcquired += HandleLock;
    }

    private void OnDisable()
    {
        owner.OnHit -= HandleHit;
        GameEvents.OnLockAcquired -= HandleLock;

        fillTween?.Kill();
        delayFillTween?.Kill();
        visibleTween?.Kill();
        punchTween?.Kill();
    }

    private void Update()
    {
        UpdateHideTimer();
        UpdateDelayDrainTimer();
    }

    private void HandleHit(float currentHealth, float maxHealth, HitData hit)
    {
        float previousFill = fillImage.fillAmount;
        targetFill = Mathf.Clamp01(currentHealth / maxHealth);
        float damageFill = previousFill - targetFill;

        if (!isVisible)
            ShowBar();

        hideTimer = hideDelay;
        delayFillTimer = delayFillDelay;
        delayDrainPending = true;

        fillTween?.Kill();
        fillTween = fillImage.DOFillAmount(targetFill, mainBarDuration).SetEase(Ease.OutQuart);

        punchTween?.Kill();
        punchTween = barRoot.DOPunchScale(Vector3.one * hitPunchScale, hitPunchDuration, 5, 0.5f);
    }

    private void HandleLock(EnemyBase enemy)
    {
        if (enemy != owner) return;

        if (!isVisible)
            ShowBar();

        hideTimer = hideDelay;
    }

    private void UpdateHideTimer()
    {
        if (alwaysVisible || !isVisible) return;
        if (owner.hasPlayerLock) return;

        if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
                HideBar();
        }
    }


    private void UpdateDelayDrainTimer()
    {
        if(!delayDrainPending) return;

        delayFillTimer -= Time.deltaTime;
        if (delayFillTimer <= 0f)
        {
            delayDrainPending = false;
            AnimateFill();
        }
    }

    private void AnimateFill()
    {
        fillTween?.Kill();

        fillTween = delayFillImage
            .DOFillAmount(targetFill, delayFillDuration)
            .SetEase(Ease.OutCubic);
    }

    private void DoDamageChunk(float damageFill)
    {
        float filledAngle = targetFill * 360f;
        
        // Instantiate a new object
        //newArc.localRotation = Quaternion.Euler(0f, 0f, -filledAngle);
        //newArcImage.fillAmount = 1f - targetFill + damageFill
    }

    private void ShowBar()
    {
        isVisible = true;

        visibleTween?.Kill();
        visibleTween = DOTween.Sequence()
            .Join(canvasGroup.DOFade(1f, showDuration).SetEase(Ease.OutQuad))
            .Join(barRoot.DOScale(1f, showDuration).SetEase(Ease.OutBack));
    }

    private void HideBar()
    {
        isVisible = false;

        visibleTween?.Kill();
        visibleTween = DOTween.Sequence()
            .Join(canvasGroup.DOFade(0f, hideDuration).SetEase(Ease.InQuad))
            .Join(barRoot.DOScale(0.8f, hideDuration).SetEase(Ease.InBack));
    }
}
