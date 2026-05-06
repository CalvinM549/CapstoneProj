using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    [SerializeField] private HealthData data;

    private Player p;

    public List<HealthSegment> healthSegments;
    private int activeHealthIndex;

    public HealthSegment activeSegment => healthSegments[activeHealthIndex];

    public bool IsAlive { get; set; }
    public bool IsIframe => isIframe;
    public bool IsHitstunned { get; private set; }

    private bool isIframe = false;


    private int currentHealth;

    private Coroutine hitStunRoutine;
    private Coroutine iFrameRoutine;

    public float PercentHealth => currentHealth / data.maxHealth;

    private void Awake()
    {
        p = GetComponent<Player>();

        IsAlive = true;
    }

    private void Start()
    {
        InitializeSegments(data.segmentBaseCount);
    }

    private void InitializeSegments(int segmentCount)
    {
        healthSegments.Clear();

        for (int i = 0; i < segmentCount; i++)
        {
            bool isActive = i == segmentCount - 1;
            healthSegments.Add(new HealthSegment(data.segmentMaxHealth, isActive));
        }

        activeHealthIndex = healthSegments.Count - 1;

        GameEvents.PlayerHealthChanged(healthSegments);
    }

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;
        if (IsIframe) return;
        if (p.Tools.TryInterceptWithTool(hit)) return;

        ApplyKnockback(hit);
        ApplyHitStun(hit);
        ApplyDamage(hit);
    }

    private void ApplyDamage(HitData hit)
    {
        GameEvents.PlayerHit(hit);

        HealthSegment segment = activeSegment;

        float overflow = segment.ReduceHealth(hit.damage);

        if (segment.IsDestroyed)
        {
            activeHealthIndex--;
            segment.IsActive = false;
            
            if (activeHealthIndex < 0)
            {
                PlayerDeath();
                return;
            }
            else
            {
                healthSegments[activeHealthIndex].IsActive = true;
            }
        }

        GameEvents.PlayerHealthChanged(healthSegments);

        GrantIFrames(data.hitIFrameDuration);
    }

    private void ApplyKnockback(HitData hit)
    {
        p.Movement.PushPlayer(hit.knockbackDirection, hit.knockbackForce, hit.hitstunTime);
    }

    private void ApplyHitStun(HitData hit)
    {
        StartCoroutine(HitStunRoutine(hit.hitstunTime));
    }

    public void GrantIFrames(float duration)
    {
        if (iFrameRoutine != null)
            StopCoroutine(iFrameRoutine);

        // Fire event?

        iFrameRoutine = StartCoroutine(IFrameRoutine(duration));
    }

    private IEnumerator IFrameRoutine(float duration)
    {
        isIframe = true;

        yield return new WaitForSeconds(duration);

        isIframe = false;
    }

    private IEnumerator HitStunRoutine(float duration)
    {
        IsHitstunned = true;

        yield return new WaitForSeconds(duration);

        IsHitstunned = false;
    }

    private void PlayerDeath()
    {
        IsAlive = false;
        isIframe = true;

        StopAllCoroutines();

        gameObject.SetActive(false);

        GameEvents.PlayerDeath();
    }

    public void AddHealthSegment()
    {

    }

    public void RemoveHealthSegment()
    {

    }
}
