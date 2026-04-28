using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public List<HealthSegment> healthSegments;
    private int activeHealthIndex;

    public HealthSegment activeSegment => healthSegments[activeHealthIndex];

    [SerializeField] private HealthData data;

    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerTools tools;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerMomentum momentum;
    [SerializeField] private PlayerAnimator animator;

    private Material baseMaterial;
    [SerializeField] private Material damageMaterial;

    [SerializeField] private SpriteRenderer sr;

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
        IsAlive = true;

        baseMaterial = sr.material;

        InitializeSegments();
    }

    private void Start()
    {
        //currentHealth = data.maxHealth;
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void InitializeSegments()
    {
        healthSegments.Clear();

        for (int i = 0; i < data.segmentBaseCount; i++)
        {
            healthSegments.Add(new HealthSegment(data.segmentMaxHealth));
        }

        activeHealthIndex = healthSegments.Count - 1;

        GameEvents.PlayerHealthChanged(healthSegments);
    }

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;
        if (IsIframe) return;
        if (tools.TryInterruptWithTool(hit)) return;

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
            if (activeHealthIndex >= healthSegments.Count - 1 || activeHealthIndex == -1)
            {
                PlayerDeath();
                return;
            }

        }

        GameEvents.PlayerHealthChanged(healthSegments);


        //currentHealth = Mathf.Max(0, currentHealth - hit.damage);

        //GameEvents.PlayerHealthChanged(currentHealth, data.maxHealth);

        //if (currentHealth <= 0)
        //{
        //    PlayerDeath();
        //    return;
        //}

        GrantIFrames(data.hitIFrameDuration);
    }

    private void ApplyKnockback(HitData hit)
    {
        movement.PushPlayer(hit.knockbackDirection, hit.knockbackForce, hit.hitstunTime);
    }

    private void ApplyHitStun(HitData hit)
    {
        StartCoroutine(HitFXRoutine(hit.hitstunTime));
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

    private IEnumerator HitFXRoutine(float duration)
    {
        // flip sprite if needed

        // change sprite to hit sprite (freeze animator??)

        sr.material = damageMaterial;
        IsHitstunned = true;

        yield return new WaitForSeconds(duration);

        sr.material = baseMaterial;
        IsHitstunned = false;
    }

    private IEnumerator HitstunRoutine()
    {
        float duration = 0.1f;

        yield return new WaitForSeconds(duration);
    }

    private void PlayerDeath()
    {
        IsAlive = false;
        isIframe = true;

        StopAllCoroutines();

        gameObject.SetActive(false);

        GameEvents.PlayerDeath();
    }
}
