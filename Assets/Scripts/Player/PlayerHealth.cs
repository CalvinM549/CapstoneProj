using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class  PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private HealthData data;
    [SerializeField] private bool UseIFrames;

    private StatValue structureCount;
    private StatValue structureHealth;

    private Player p;

    private StatusEffectController statusController;

    public List<HealthSegment> healthSegments;
    private int activeHealthIndex;

    public HealthSegment ActiveSegment => healthSegments[activeHealthIndex];
    public int ActiveHealthIndex => activeHealthIndex;

    public bool IsAlive { get; set; }
    public bool IsIframe => isIframe;
    public bool IsHitstunned { get; private set; }

    private bool isIframe = false;

    private Coroutine hitStunRoutine;
    private Coroutine iFrameRoutine;

    public void Initialize()
    {
        structureHealth = p.Stats.GetStatValue(StatRef.PlayerBaseStructureHealth);
        structureHealth.OnChanged += HandleStructureHealthChanged;
        structureCount = p.Stats.GetStatValue(StatRef.PlayerStructureCount);

        p.Stats.Subscribe(StatRef.PlayerStructureCount, HandleStructureCountStatChanged);

        SetSegmentCount(structureCount.ValueInt);
    }

    public void RestoreFromSave(int activeIndex, List<SegmentSave> savedSegments)
    {
        SetSegmentCount(savedSegments.Count);
        activeHealthIndex = Mathf.Clamp(activeIndex, 0, healthSegments.Count - 1);

        for (int i = 0; i < healthSegments.Count && i < savedSegments.Count; i++)
        {
            healthSegments[i].RestoreFromSave(savedSegments[i]);
        }

        GameEvents.PlayerHealthChanged(healthSegments);
    }

    private void Awake()
    {
        p = GetComponent<Player>();
        statusController = GetComponent<StatusEffectController>();

        IsAlive = true;
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        structureHealth.OnChanged -= HandleStructureHealthChanged;
        p.Stats.Unsubscribe(StatRef.PlayerStructureCount, HandleStructureCountStatChanged);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.J))
        {
            RestoreSegments(2);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            var structureCountBuff = new StatModifier(1, StatModType.Flat);
            structureCount.AddStatModifier(structureCountBuff);
        }
#endif
    }

    private void HandleStructureCountStatChanged()
    {
        SetSegmentCount(structureCount.ValueInt);
    }

    private void SetSegmentCount(int newCount)
    {
        newCount = Mathf.Max(newCount, 0);
        int oldCount = healthSegments.Count;
        if (oldCount == newCount) return;

        if (newCount > oldCount)
        {
            GrantSegments(newCount);
        }
        else
        {
            LoseSegments(newCount);
        }

        GameEvents.PlayerHealthChanged(healthSegments);
    }

    private void GrantSegments(int newCount)
    {
        if (healthSegments.Count > 0)
            ActiveSegment.IsActive = false;

        int toAdd = newCount - healthSegments.Count;
        for (int i = 0; i < toAdd; i++)
        {
            healthSegments.Insert(0, new HealthSegment(structureHealth.Value, false));
        }

        activeHealthIndex = healthSegments.Count - 1;
        ActiveSegment.IsActive = true;
    }

    private void LoseSegments(int newCount)
    {
        while (healthSegments.Count > newCount && healthSegments.Count - 1 > activeHealthIndex)
            healthSegments.RemoveAt(healthSegments.Count - 1);

        while (healthSegments.Count > newCount && healthSegments.Count > 0)
        {
            healthSegments.RemoveAt(healthSegments.Count - 1);
            activeHealthIndex = Mathf.Min(activeHealthIndex, healthSegments.Count - 1);
        }

        if (healthSegments.Count > 0)
            ActiveSegment.IsActive = true;
        else
            PlayerDeath();
    }

    private void HandleStructureHealthChanged()
    {

    }

    #region Recieving Damage

    public void RecieveHit(HitData hit)
    {
        if (DebugManager.GodMode) return;

        if (!IsAlive) return;
        if (IsIframe) return;
        if (p.Tools.TryInterceptWithTool(hit)) return; // For parry tools etc
        // Do event trigger for upgrades? make event return true maybe

        ApplyKnockback(hit);
        ApplyHitStun(hit);
        ApplyDamage(hit);
        ApplyStatuses(hit);
    }

    private void ApplyDamage(HitData hit)
    {
        float defenceMult = p.Stats.Get(StatRef.PlayerIncomingDamageMult);
        hit.AddModifier(defenceMult - 1, StatModType.PercentAdd, this);

        p.Upgrades.ModifyIncomingHit(hit);
        GameEvents.PlayerRecievedHit(hit);

        HealthSegment segment = ActiveSegment;

        segment.ReduceHealth(hit.FinalDamage); // overflow value unused atm

        GameEvents.PlayerTookDamage(hit);

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

    private void ApplyStatuses(HitData hit)
    {
        foreach (var pending in hit.PendingStatusEffects)
        {
            statusController?.ApplyEffects(pending.data, pending.source, pending.appliedByPlayer, pending.stacks);
        }
    }

    private void ApplyKnockback(HitData hit)
    {
        p.Movement.PushPlayer(hit.knockbackDirection, hit.knockbackForce, hit.hitstunTime);
    }

    private void ApplyHitStun(HitData hit)
    {
        if(hitStunRoutine != null)
            StopCoroutine(hitStunRoutine);

        hitStunRoutine = StartCoroutine(HitStunRoutine(hit.hitstunTime));
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

    #region Restoring Health

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

    public void RestoreSegments(int count)
    {
        print($"restoring {count} segments");

        if (count <= 0) return;

        int healed = 0;

        for (int index = 0; index < healthSegments.Count && healed < count; index++)
        {
            HealthSegment currentSegment = healthSegments[index];
            if (!currentSegment.IsDestroyed) continue;

            currentSegment.RestoreSegment();
            healthSegments.Remove(currentSegment);
            healthSegments.Insert(0, currentSegment);

            activeHealthIndex++;
            healed++;
            print($"Restoring segment at index : {index}, healed = {healed}");
        }

        if(healed < count)
            RestoreCurrentSegment();

        //Debug.Log($"[PlayerHealth] toHeal: {count} | healed: {healed}");

        if(healed > 0)
            GameEvents.PlayerHealthChanged(healthSegments);
    }

    #endregion

    private void PlayerDeath()
    {
        GameEvents.PlayerHealthChanged(healthSegments);
        IsAlive = false;
        isIframe = true;

        StopAllCoroutines();

        GameEvents.PlayerDeath();
        GameEvents.RunEnded(false);
    }
}
