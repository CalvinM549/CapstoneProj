using System.Collections;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAnimator animator;

    private ObjectPool<FadingSprite> afterImagePool;
    [SerializeField] private FadingSprite afterImagePrefab;
    [SerializeField] private Transform vfxContainer;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();

        afterImagePool = new ObjectPool<FadingSprite>(afterImagePrefab, 5, vfxContainer);
    }

    public void PlayDashTrail()
    {
        StartCoroutine(DashTrailRoutine());
    }

    private IEnumerator DashTrailRoutine()
    {
        while (movement.IsDashing)
        {
            var afterImage = afterImagePool.Get();
            afterImage.transform.position = transform.position;
            afterImage.Initialize(animator.currentSprite, afterImagePool, sr.flipX);

            yield return new WaitForSeconds(0.05f);
        }
    }

}
