using DG.Tweening;
using System;
using UnityEngine;

public class SimpleTelegraph : MonoBehaviour
{
    [SerializeField] private Transform outline;
    [SerializeField] private Transform fill;

    [SerializeField] private Ease fillEase;
    [SerializeField] private bool destroyOnComplete = true;

    private Tween fillTween;

    public void Play(Vector2 position, float radius, float duration, Action onComplete = null)
    {
        transform.position = position;

        float diameter = radius * 2f;
        Vector2 fullScale = new(diameter, diameter);

        if (outline != null)
            outline.localScale = fullScale;

        fillTween?.Kill();

        if (fill != null)
        {
            fill.localScale = Vector2.zero;
            fillTween = fill.DOScale(fullScale, duration)
                .SetEase(fillEase)
                .SetLink(gameObject)
                .OnComplete(() => Finish(onComplete));
        }
        else
        {
            fillTween = DOVirtual.DelayedCall(duration, () => Finish(onComplete))
                .SetLink(gameObject);
        }
    }

    public void Cancel()
    {
        fillTween?.Kill();
        Destroy(gameObject);
    }

    public void Finish(Action onComplete)
    {
        onComplete?.Invoke();

        if(destroyOnComplete)
            Destroy(gameObject);
    }
}
