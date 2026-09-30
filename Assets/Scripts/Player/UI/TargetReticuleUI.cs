
using DG.Tweening;
using UnityEngine;

public class EnemyTargetedUI : MonoBehaviour
{
    private EnemyController owner;

    [SerializeField] private CanvasGroup cg;
    [SerializeField] private float rotationSpeed;

    [SerializeField] private RectTransform rotationAnchor;

    private void Awake()
    {
        owner = GetComponentInParent<EnemyController>();

        cg.alpha = 0f;
    }

    private void OnEnable()
    {
        owner.onLockChange += HandleLockChange;
    }

    private void OnDisable()
    {
        owner.onLockChange -= HandleLockChange;

        cg.DOKill();
    }

    private void Update()
    {
        if (!owner.hasPlayerLock) return;

        rotationAnchor.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void HandleLockChange(bool locked)
    {
        cg.DOKill();

        float targetValue = locked ? 0.6f : 0f;

        cg.DOFade(targetValue, 0.25f).SetEase(Ease.OutBack);
    }
}
