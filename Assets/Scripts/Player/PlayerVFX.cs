using System.Collections;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;

    private ObjectPool<FadingSprite> afterImagePool;
    [SerializeField] private FadingSprite afterImagePrefab;
    [SerializeField] private Transform vfxContainer;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

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
            afterImage.Initialize(sr.sprite, afterImagePool, sr.flipX);

            yield return new WaitForSeconds(0.05f);
        }
    }

}
