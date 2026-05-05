using System.Collections;
using UnityEngine;

public class PlayerVFX : MonoBehaviour
{
    private Player p;

    private ObjectPool<FadingSprite> afterImagePool;
    [SerializeField] private FadingSprite afterImagePrefab;
    [SerializeField] private Transform vfxContainer;

    [SerializeField] private ParticleSystem steamParticles;

    private void Awake()
    {
        p = GetComponent<Player>();

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

    #region Dash Trail

    public void PlayDashTrail()
    {
        StartCoroutine(DashTrailRoutine());
    }

    private IEnumerator DashTrailRoutine()
    {
        while (p.Movement.IsDashing)
        {
            var afterImage = afterImagePool.Get();
            afterImage.transform.position = p.Animator.spritePos;
            afterImage.Initialize(p.Animator.currentSprite, afterImagePool, p.Animator.IsFacingRight);

            yield return new WaitForSeconds(0.05f);
        }
    }

    #endregion

    #region Steam Emission

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

    #endregion

}
