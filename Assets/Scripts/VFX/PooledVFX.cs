using System.Collections;
using UnityEngine;

public class PooledVFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particles;

    [SerializeField] private float lifetime = 1f; // used if no particles exist

    private ObjectPool<PooledVFX> ownerPool;
    private Coroutine returnRoutine;

    private void Awake()
    {
        if (particles == null || particles.Length == 0)
            particles = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void Play(ObjectPool<PooledVFX> pool, Vector3 position, Quaternion rotation)
    {
        ownerPool = pool;
        transform.SetPositionAndRotation(position, rotation);

        if (particles != null)
        {
            foreach (var particle in particles)
            {
                particle.Clear();
                particle.Play();
            }
        }

        if(returnRoutine != null)
            StopCoroutine(returnRoutine);

        returnRoutine = StartCoroutine(ReturnAfterDelay());
    }

    private IEnumerator ReturnAfterDelay()
    {
        if (particles != null && particles.Length > 0)
        {
            yield return new WaitUntil(() => !AnyParticlesAlive());
        }
        else
        {
            yield return new WaitForSeconds(lifetime);
        }

        returnRoutine = null;
        ownerPool?.ReturnToPool(this);
    }

    private bool AnyParticlesAlive()
    {
        foreach (var particle in particles)
        {
            if(particle.IsAlive(true)) return true;
        }
        return false;
    }

    public void ForceReturn()
    {
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }

        if (particles != null)
            foreach (var particle in particles)
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ownerPool?.ReturnToPool(this);
    }
}
