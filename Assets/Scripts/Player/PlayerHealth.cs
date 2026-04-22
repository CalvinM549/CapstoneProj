using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public List<HealthSegment> healthSegments;
    public HealthSegment activeSegment;

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
    }

    private void Start()
    {

        currentHealth = data.maxHealth;

        for (int i = 0; i < data.integrityBaseCount; i++)
        {
            // Populate integrity lists
        }

    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
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

        //float bleedThrough = hit.momentumCanBlock ? momentum.DrainAsBuffer(hit.momentumCost) : 1f;

        //if (bleedThrough <= 0)
        //{
        //    GrantIFrames(data.hitIFrameDuration * 0.5f);
        //    return;
        //}

        //float finalDamage = hit.damage * bleedThrough;


        
        currentHealth = Mathf.Max(0, currentHealth - hit.damage);

        GameEvents.PlayerHealthChanged(currentHealth, data.maxHealth);

        if (currentHealth <= 0)
        {
            PlayerDeath();
            return;
        }

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
