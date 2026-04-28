using System.Collections;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAnimator animator;

    [SerializeField] private SpriteRenderer sr;

    private ObjectPool<FadingSprite> afterImagePool;
    [SerializeField] private FadingSprite afterImagePrefab;
    [SerializeField] private Transform vfxContainer;

    [SerializeField] private ParticleSystem steamParticles;

    private void Awake()
    {

        afterImagePool = new ObjectPool<FadingSprite>(afterImagePrefab, 5, vfxContainer);
        StopSteamParticles();
    }

    private void OnEnable()
    {
        GameEvents.OnPassiveDrainStart += StartSteamParticles;
        GameEvents.OnPassiveDrainEnd += StopSteamParticles;
    }

    private void OnDisable()
    {
        GameEvents.OnPassiveDrainStart -= StartSteamParticles;
        GameEvents.OnPassiveDrainEnd -= StopSteamParticles;
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
            afterImage.transform.position = sr.transform.position;
            afterImage.Initialize(animator.currentSprite, afterImagePool, animator.IsFacingRight);

            yield return new WaitForSeconds(0.05f);
        }
    }

    private void StartSteamParticles()
    {
        if(!steamParticles.isPlaying)
            steamParticles.Play();
    }

    private void StopSteamParticles()
    {
        if(steamParticles.isPlaying)
            steamParticles.Stop();
    }

}
