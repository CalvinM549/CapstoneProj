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

    private EnemyBase owner;
    private float barWidth;

    private float targetFill = 1f;
    private float damageTimer;
    bool barDrainStarted;

    private Tween fillTween;

    private void Awake()
    {
        owner = GetComponentInParent<EnemyBase>();

        barWidth = barRoot.rect.width;
    }

    private void OnEnable()
    {
        owner.OnHit += HandleHit;
    }

    private void OnDisable()
    {

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

    private void UpdateTimer()
    {

    }

    #region Damage Chunks

    private void SpawnDamageChunk(float startFill, float chunkFill)
    {
        if (chunkPrefab == null || chunkContainer == null) return;

        Image chunk = Instantiate(chunkPrefab, chunkContainer);

        RectTransform rt = chunk.rectTransform;
        rt.anchorMax = Vector3.zero;
        rt.anchorMin = Vector3.zero;
        rt.pivot = Vector3.zero;

        float leftEdge = (startFill - chunkFill) * barWidth;
        float chunkWidth = chunkFill * barWidth;
        float barHeight = barRoot.rect.height;

        rt.anchoredPosition = new Vector2(leftEdge, 0f);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, chunkWidth);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);

        CanvasGroup cg = chunk.gameObject.AddComponent<CanvasGroup>();

        cg.DOFade(0f, 0.5f);
    }

    #endregion
}
