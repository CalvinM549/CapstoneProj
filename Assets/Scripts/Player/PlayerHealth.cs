using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private HealthData data;
    [SerializeField] private bool UseIFrames;
    [SerializeField] private bool structurePierce;

    private StatValue structureCount;
    private StatValue structureHealth;

    private Player p;
    private StatusEffectController statusController;

    public List<HealthSegment> healthSegments = new();
    private int activeHealthIndex;

    public HealthSegment ActiveSegment => healthSegments[activeHealthIndex];
    public int ActiveHealthIndex => activeHealthIndex;

    public bool IsAlive { get; set; }
    public bool IsIframe => !IsAlive || Time.time < iFrameEndTime;
    public bool IsHitstunned => Time.time < hitStunEndTime;


    private float iFrameEndTime;
    private float hitStunEndTime;

    private bool initialized;
    private bool subscribed;


    private void Awake()
    {
        p = GetComponent<Player>();
        statusController = GetComponent<StatusEffectController>();

        IsAlive = true;
    }

    private void OnEnable()
    {
        if (initialized) Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Initialize()
    {
        structureHealth = p.Stats.GetStatValue(StatRef.PlayerBaseStructureHealth);
        structureCount = p.Stats.GetStatValue(StatRef.PlayerStructureCount);
        initialized = true;

        Subscribe();
        SetSegmentCount(structureCount.ValueInt);
    }

    private void Subscribe()
    {
        if (subscribed) return;

        structureHealth.OnChanged += HandleStructureHealthChanged;
        p.Stats.Subscribe(StatRef.PlayerStructureCount, HandleStructureCountStatChanged);

        subscribed = true;

    }

    private void Unsubscribe()
    {
        if (!subscribed) return;

        structureHealth.OnChanged -= HandleStructureHealthChanged;
        p.Stats.Unsubscribe(StatRef.PlayerStructureCount, HandleStructureCountStatChanged);

        subscribed = false;
    }

    public void RestoreFromSave(PlayerHealthSave save)
    {
        SetSegmentCount(save.segmentHealth.Count);
        activeHealthIndex = Mathf.Clamp(save.activeSegmentIndex, 0, healthSegments.Count - 1);

        for (int i = 0; i < healthSegments.Count && i < save.segmentHealth.Count; i++)
            healthSegments[i].currentHealth = save.segmentHealth[i];

        SyncActiveFlags();
        GameEvents.PlayerHealthChanged(healthSegments);
    }

    public PlayerHealthSave ToSave()
    {
        PlayerHealthSave save = new()
        {
            activeSegmentIndex = activeHealthIndex,
        };

        for (int i = 0; i < healthSegments.Count; i++)
        {
            var segment = healthSegments[i];
            save.segmentHealth.Add(segment.currentHealth);
        }

        return save;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            RestoreSegments(2);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            var structureCountBuff = new StatModifier(1, StatModType.Flat);
            structureCount.AddStatModifier(structureCountBuff);
        }
    }
#endif

    #region Segment Count

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
            GrantSegments(newCount);

        else
            LoseSegments(newCount);

        GameEvents.PlayerHealthChanged(healthSegments);
    }

    private void GrantSegments(int newCount)
    {
        bool wasEmpty = healthSegments.Count == 0;
        int toAdd = newCount - healthSegments.Count;
        
        for (int i = 0; i < toAdd; i++)
            healthSegments.Insert(0, new HealthSegment(structureHealth.Value));

        activeHealthIndex = wasEmpty ? healthSegments.Count - 1 : activeHealthIndex + toAdd;
        SyncActiveFlags();
    }

    private void LoseSegments(int newCount)
    {
        while (healthSegments.Count > newCount && healthSegments.Count - 1 > activeHealthIndex)
            healthSegments.RemoveAt(healthSegments.Count - 1);

        while (healthSegments.Count > newCount && healthSegments.Count > 0)
            healthSegments.RemoveAt(healthSegments.Count - 1);

        if (healthSegments.Count > 0)
            SyncActiveFlags();
        else
            Die();
    }

    private void HandleStructureHealthChanged()
    {
        float newMax = structureHealth.Value;

        foreach (var segment in healthSegments)
        {
            if (Mathf.Approximately(segment.currentHealth, segment.maxHealth))
                segment.currentHealth = newMax;

            segment.maxHealth = newMax;
        }

        GameEvents.PlayerHealthChanged(healthSegments);
    }

    private void SyncActiveFlags()
    {
        for (int i = 0; i < healthSegments.Count; i++)
            healthSegments[i].IsActive = (i == activeHealthIndex);
    }

    #endregion

    #region Recieving Damage

    public void RecieveHit(HitData hit)
    {
        if (DebugManager.GodMode) return;

        if (!IsAlive) return;
        if (p.Tools.TryInterceptWithTool(hit)) return; // For parry tools etc

        p.Upgrades.ModifyIncomingHit(hit);
        GameEvents.PlayerRecievedHit(hit);

        if(!ApplyDamage(hit)) return;

        ApplyStatuses(hit);

        if (hit.attackType != AttackType.DamageOverTime)
        {
            ApplyKnockback(hit);
            ApplyHitStun(hit);
            ScreenGlitch.Instance.Pulse(hit.hitstopTime, 0.4f);
        }
    }

    private bool ApplyDamage(HitData hit)
    {
        float defenceMult = p.Stats.Get(StatRef.PlayerIncomingDamageMult);
        hit.AddModifier(defenceMult - 1, StatModType.PercentAdd, this);

        float remaining = hit.FinalDamage;
        int spillsLeft = structurePierce ? Mathf.Max(0, 999) : 0;

        while (true)
        {
            HealthSegment segment = ActiveSegment;
            remaining = segment.ReduceHealth(remaining);

            if (!segment.IsDestroyed) break;

            if (activeHealthIndex == 0)
            {
                SyncActiveFlags();
                Die();
                return false;
            }

            activeHealthIndex--;

            if (remaining <= 0 || spillsLeft <= 0 || activeHealthIndex == 0)
                break;

            spillsLeft--;
        }

        SyncActiveFlags();

        GameEvents.PlayerTookDamage(hit);
        GameEvents.PlayerHealthChanged(healthSegments);

        return true;
    }

    private void ApplyStatuses(HitData hit)
    {
        foreach (var pending in hit.PendingStatusEffects)
            statusController?.ApplyEffects(pending.data, pending.source, pending.appliedByPlayer, pending.stacks);
    }

    private void ApplyKnockback(HitData hit)
    {
        p.Movement.PushPlayer(hit.knockbackDirection, hit.knockbackForce, hit.hitstunTime);
    }

    private void ApplyHitStun(HitData hit)
    {
        hitStunEndTime = Time.time + hit.hitstunTime;
    }

    public void GrantIFrames(float duration)
    {
        iFrameEndTime = Mathf.Max(iFrameEndTime, Time.time + duration);
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
        if (count <= 0 || healthSegments.Count == 0) return;
        int healed = 0;

        while (healed < count && activeHealthIndex < healthSegments.Count - 1)
        {
            int last = healthSegments.Count - 1;
            HealthSegment segment = healthSegments[last];

            healthSegments.RemoveAt(last);
            segment.RestoreSegment();
            healthSegments.Insert(0, segment);

            activeHealthIndex++;
            healed++;
        }

        if(healed < count)
            RestoreCurrentSegment();

        if(healed > 0)
            GameEvents.PlayerHealthChanged(healthSegments);
    }

    #endregion

    private void Die()
    {
        GameEvents.PlayerHealthChanged(healthSegments);
        VFXManager.Instance.PlayVFX(VFXType.ExplosionComplex, transform.position);

        IsAlive = false;

        StopAllCoroutines();

        GameEvents.PlayerDeath();
        GameEvents.RunEnded(false);
    }
}
