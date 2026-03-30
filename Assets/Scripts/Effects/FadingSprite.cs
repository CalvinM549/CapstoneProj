using DG.Tweening;
using UnityEngine;

public class FadingSprite : MonoBehaviour
{
    [SerializeField] private float lifetime;

    private ObjectPool<FadingSprite> pool;

    private SpriteRenderer sr;

    public void Initialize(Sprite sprite, ObjectPool<FadingSprite> pool, bool doFlip)
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.flipX = doFlip;
        sr.color = Color.white;

        sr.DOFade(0, lifetime).OnComplete(() => pool.ReturnToPool(this));
    }

}
