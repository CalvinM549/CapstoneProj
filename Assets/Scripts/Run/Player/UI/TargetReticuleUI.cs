
using DG.Tweening;
using UnityEngine;

public class TargetReticleUI : MonoBehaviour
{
    private EnemyBase owner;

    [SerializeField] private CanvasGroup cg;
    [SerializeField] private float rotationSpeed;


    [SerializeField] private RectTransform rotationAnchor;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        owner = GetComponentInParent<EnemyBase>();

        cg.alpha = 0f;
    }

    private void OnEnable()
    {
        GameEvents.OnLockAcquired += HandleLockAcquired;
        GameEvents.OnLockDropped += HandleLockDropped;
    }

    private void OnDisable()
    {
        GameEvents.OnLockAcquired -= HandleLockAcquired;
        GameEvents.OnLockDropped -= HandleLockDropped;

        cg.DOKill();
    }

    private void Update()
    {
        if (!owner.hasPlayerLock) return;

        rotationAnchor.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    private void HandleLockAcquired(EnemyBase enemy)
    {
        if (enemy != owner) return;

        owner.hasPlayerLock = true;

        cg.DOKill();
        cg.DOFade(0.6f, 0.3f).SetEase(Ease.OutBack);

    }

    private void HandleLockDropped()
    {
        if (!owner.hasPlayerLock) return;

        owner.hasPlayerLock = false;

        cg.DOKill();
        cg.DOFade(0f, 0.1f);
    }

}
