using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{

    [SerializeField] private HealthData data;
    [SerializeField] private bool UseIFrames;

    private Player p;

    public List<HealthSegment> healthSegments;
    private int activeHealthIndex;

    public HealthSegment ActiveSegment => healthSegments[activeHealthIndex];
    public int ActiveHealthIndex => activeHealthIndex;

    public bool IsAlive { get; set; }
    public bool IsIframe => isIframe;
    public bool IsHitstunned { get; private set; }

    private bool isIframe = false;

    private bool isInitialized = false;

    private Coroutine hitStunRoutine;
    private Coroutine iFrameRoutine;


    private void Awake()
    {
        p = GetComponent<Player>();

        IsAlive = true;
    }

    private void Start()
    {
        InitializeHealth(data.segmentBaseCount);
    }

    public void Initialize()
    {
        InitializeHealth(data.segmentBaseCount);

    }

    private void InitializeHealth(int segmentCount)
    {

        if (isInitialized) return;

        healthSegments.Clear();

        for (int i = 0; i < segmentCount; i++)
        {
            bool isActive = i == segmentCount - 1;
            healthSegments.Add(new HealthSegment(data.segmentMaxHealth, isActive));
        }

        activeHealthIndex = healthSegments.Count - 1;

        GameEvents.PlayerHealthChanged(healthSegments);

        isInitialized = true;
    }

    public void Restore(int activeIndex, List<SegmentSave> savedSegments)
    {
        InitializeHealth(savedSegments.Count);
        activeHealthIndex = Mathf.Clamp(activeIndex, 0, healthSegments.Count - 1);

        for (int i = 0; i < healthSegments.Count && i < savedSegments.Count; i++)
        {
            healthSegments[i].RestoreFromSave(savedSegments[i]);
        }

        GameEvents.PlayerHealthChanged(healthSegments);
    }

    public void RestoreCurrentSegment()
    {
        if (ActiveSegment == null)
        {
            Debug.LogWarning("[PlayerHealth] No active health segment found");
            return;
        }
        ActiveSegment.RestoreSegment();
        GameEvents.PlayerHealthChanged(healthSegments);
    }

    #region Recieving Damage

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;
        if (IsIframe) return;
        if (p.Tools.TryInterceptWithTool(hit)) return; // For parry tools etc
        // Do event trigger for upgrades? make event return true maybe

        ApplyKnockback(hit);
        ApplyHitStun(hit);
        ApplyDamage(hit);
    }

    private void ApplyDamage(HitData hit)
    {
        GameEvents.PlayerHit(hit);

        HealthSegment segment = ActiveSegment;

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

        if(UseIFrames)
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

    #endregion

    private void PlayerDeath()
    {
        GameEvents.PlayerHealthChanged(healthSegments);
        IsAlive = false;
        isIframe = true;

        StopAllCoroutines();

        GameEvents.PlayerDeath();
    }
}
