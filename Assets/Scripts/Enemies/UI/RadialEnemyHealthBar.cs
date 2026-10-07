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

    [SerializeField] private float circleScale = 1f;

    [SerializeField] private float mainBarDuration;

    [SerializeField] private float delayFillDelay;
    [SerializeField] private float delayFillDuration;

    [SerializeField] private float hitPunchScale;
    [SerializeField] private float hitPunchDuration;

    private EnemyController owner;
    private CanvasGroup canvasGroup;

    private float targetFill = 1f;
    private float delayFillTimer;

    private bool delayDrainPending;

    private Tween fillTween;
    private Tween delayFillTween;
    private Tween punchTween;

    private void Awake()
    {
        owner = GetComponentInParent<EnemyController>();
        canvasGroup = barRoot.GetComponent<CanvasGroup>();
    }


    private void OnEnable()
    {
        owner.OnHit += HandleHit;
    }

    private void OnDisable()
    {
        owner.OnHit -= HandleHit;

        fillTween?.Kill();
        delayFillTween?.Kill();
        punchTween?.Kill();
    }

    private void Update()
    {
        UpdateDelayDrainTimer();
    }

    public void Initialize()
    {
        canvasGroup.alpha = 1f;
        barRoot.localScale = Vector3.one * 0.8f;

        fillImage.rectTransform.localScale *= circleScale;
        delayFillImage.rectTransform.localScale *= circleScale;
    }

    private void HandleHit(float currentHealth, float maxHealth, HitData hit)
    {
        float previousFill = fillImage.fillAmount;
        targetFill = Mathf.Clamp01(currentHealth / maxHealth);
        float damageFill = previousFill - targetFill;

        delayFillTimer = delayFillDelay;
        delayDrainPending = true;

        fillTween?.Kill();
        fillTween = fillImage.DOFillAmount(targetFill, mainBarDuration).SetEase(Ease.OutQuart);

        if (hit.attackType != AttackType.DamageOverTime)
        {
            punchTween?.Kill();
            punchTween = barRoot.DOPunchScale(Vector3.one * hitPunchScale, hitPunchDuration, 5, 0.5f);
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
}
