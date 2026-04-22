using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform barRoot;
    [SerializeField] private RectTransform chunkContainer;
    [SerializeField] private Image chunkPrefab;

    private EnemyBase owner;
    private float barWidth;

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
    }

    private void HandleHit(float currentHealth, float maxHealth, HitData hit)
    {
        float previousFill = fillImage.fillAmount;
        float targetFill = Mathf.Clamp01(currentHealth / maxHealth);
        float damageFill = previousFill - targetFill;

        fillImage.fillAmount = targetFill;

        //SpawnDamageChunk(previousFill, damageFill);
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
