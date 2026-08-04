using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class HitMarker : MonoBehaviour
{
    public Image image;
    public CanvasGroup canvasGroup;

    public Vector3 Direction {  get; private set; }
    public float Age { get; private set; }

    private RectTransform rect;
    private Tween sequence;
    private Action<HitMarker> returnToPool;

    private void Awake()
    {
        rect= GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponentInChildren<Image>();
    }

    private void Update()
    {
        if (gameObject.activeSelf)
            Age += Time.deltaTime;
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }

    public void Activate(Vector2 direction, float markerRadius, float peakPunchScale, 
        float holdDuration, float fadeDuration, Color colour, Action<HitMarker> returnToPoolCallback)
    {
        Direction = direction;
        Age = 0f;
        returnToPool = returnToPoolCallback;

        canvasGroup.alpha = 1f;
        rect.localScale = Vector3.one;
        image.color = colour;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rect.localRotation = Quaternion.Euler(new Vector3(0, 0, (angle + 90)));

        image.rectTransform.anchoredPosition = new Vector2(0f, markerRadius);
        PlaySequence(peakPunchScale, holdDuration, fadeDuration);
    }

    public void Merge(float peakPunchScale, float holdDuration, float fadeDuration, Color colour)
    {
        Age = 0f;
        canvasGroup.alpha = 1f;
        rect.localScale = Vector3.one;
        image.color = colour;
        PlaySequence(peakPunchScale, holdDuration, fadeDuration);
    }

    private void PlaySequence(float peakPunchScale, float holdDuration, float fadeDuration)
    {
        sequence?.Kill();

        sequence = DOTween.Sequence()
            .Append(rect.DOPunchScale(Vector3.one * peakPunchScale, 0.2f, 6, 0.5f))
            .AppendInterval(holdDuration)
            .Append(canvasGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad))
            .Join(rect.DOScale(0.3f, fadeDuration).SetEase(Ease.InBack))
            .OnComplete(() => returnToPool?.Invoke(this));
    }

    public void ForceReturn()
    {
        sequence?.Kill();
        returnToPool?.Invoke(this);
    }

}
