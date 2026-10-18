using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RadialEnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image mainFillBar;
    [SerializeField] private Image backFillBar;

    [SerializeField] private RectTransform barRoot;

    [Header("Config")]

    [SerializeField] private float circleScaleAdjustment = 1f;

    [SerializeField] private float backFillDelay;
    [SerializeField] private float backFillSpeed;

    [SerializeField] private float hitPunchScale;
    [SerializeField] private float hitPunchDuration;

    private EnemyController owner;
    private CanvasGroup canvasGroup;

    private float targetFill = 1f;

    private Tween punchTween;

    private float delayFillTarget;
    private Coroutine activeDrainRoutine;

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

        punchTween?.Kill();
    }

    private void Update()
    {
        //UpdateDelayDrainTimer();
    }

    public void Initialize()
    {
        punchTween?.Kill();

        mainFillBar.fillAmount = 1f;
        backFillBar.fillAmount = 1f;

        canvasGroup.alpha = 1f;
        barRoot.localScale = Vector3.one * circleScaleAdjustment;
    }

    private void HandleHit(float currentHealth, float maxHealth, HitData hit)
    {
        float previousFill = mainFillBar.fillAmount;
        targetFill = Mathf.Clamp01(currentHealth / maxHealth);

        SetInstant(targetFill);
        StartCoroutine(QueueDelayedFill(targetFill));

        if (hit.attackType != AttackType.DamageOverTime)
        {
            punchTween?.Kill();
            barRoot.localScale = Vector3.one * circleScaleAdjustment * hitPunchScale;
            punchTween = barRoot.DOScale(Vector3.one * circleScaleAdjustment, hitPunchDuration);
        }
    }

    private void SetInstant(float targetFill)
    {
        mainFillBar.fillAmount = targetFill;
    }

    private IEnumerator QueueDelayedFill(float targetFill)
    {
        yield return new WaitForSeconds(backFillDelay);

        delayFillTarget = Mathf.Max(mainFillBar.fillAmount, targetFill);

        if (activeDrainRoutine == null)
            activeDrainRoutine = StartCoroutine(DelayFillRoutine());
    }

    private IEnumerator DelayFillRoutine()
    {
        while (backFillBar.fillAmount > delayFillTarget)
        {
            backFillBar.fillAmount = Mathf.MoveTowards(backFillBar.fillAmount, delayFillTarget, backFillSpeed * Time.deltaTime);

            yield return null;
        }

        backFillBar.fillAmount = delayFillTarget;
        activeDrainRoutine = null;
    }

}
