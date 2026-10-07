using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Image delayFillImage;
    
    [SerializeField] private RectTransform barRoot;

    [SerializeField] private RectTransform chunkContainer;
    [SerializeField] private Image chunkPrefab;

    [SerializeField] private float delayFillDuration;
    [SerializeField] private float timeUntilDrain;

    private EnemyController owner;
    private float barWidth;

    private float targetFill = 1f;
    private float damageTimer;
    bool barDrainStarted;

    private Tween fillTween;

    private void Awake()
    {
        owner = GetComponentInParent<EnemyController>();

        barWidth = barRoot.rect.width;
    }

    private void OnEnable()
    {
        owner.OnHit += HandleHit;
    }

    private void OnDisable()
    {
        if(owner != null)
            owner.OnHit -= HandleHit;
        fillTween?.Kill();
    }

    private void Update()
    {
        if (damageTimer > 0)
        {
            damageTimer -= Time.deltaTime;
        }
        else if (!barDrainStarted)
        {
            barDrainStarted = true;
            AnimateFill(targetFill);
        }
    }

    private void HandleHit(float currentHealth, float maxHealth, HitData hit)
    {
        damageTimer = timeUntilDrain;
        barDrainStarted = false;

        targetFill = Mathf.Clamp01(currentHealth / maxHealth);

        float previousFill = fillImage.fillAmount;
        float damageFill = previousFill - targetFill;

        fillImage.fillAmount = targetFill;

        //AnimateFill(targetFill);

        //SpawnDamageChunk(previousFill, damageFill);
    }

    private void AnimateFill(float targetFill)
    {
        fillTween?.Kill();
        fillTween = delayFillImage.DOFillAmount(targetFill, delayFillDuration).SetEase(Ease.OutCubic);
    }
}
