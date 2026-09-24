using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
    private IDamageable damageable;
    private IStatModifiable stats;

    private readonly Dictionary<StatusEffectData, ActiveStatusEffect> activeEffects = new();

    private readonly List<StatusEffectData> keysSnapshot = new();
    private readonly List<StatusEffectData> expiredThisFrame = new();

    public event Action OnEffectsChanged;
    public IReadOnlyCollection<ActiveStatusEffect> AllActiveEffects => activeEffects.Values;

    private void Awake()
    {
        damageable = GetComponent<IDamageable>();
        stats = GetComponent<IStatModifiable>();

        if (stats == null)
            Debug.LogWarning($"[StatusEffectController] No IStatModifiable found on '{name}' — " +
                              "stat-modifying effects applied here will silently skip their stat changes.");

        if (damageable == null)
            Debug.LogWarning($"[StatusEffectController] No IDamageable found on '{name}' — " +
                              "damage/heal-over-time effects applied here will silently skip ticking.");

    }

    private void OnDisable()
    {
        
    }

    private void Update()
    {
        if (TimescaleManager.IsPaused) return;
        if(activeEffects.Count == 0) return;

        keysSnapshot.Clear();
        keysSnapshot.AddRange(activeEffects.Keys);

        expiredThisFrame.Clear();

        foreach (var data in keysSnapshot)
        {
            if (!activeEffects.TryGetValue(data, out var instance)) continue;

            instance.remainingDuration -= Time.deltaTime;

            instance.tickTimer -= Time.deltaTime;

            if (instance.tickTimer <= 0f)
            {
                instance.tickTimer += data.tickInterval;
                TickEffect(data, instance);
            }

            if(instance.remainingDuration <= 0f)
                expiredThisFrame.Add(data);
        }

        foreach (var data in expiredThisFrame)
            RemoveEffect(data);
    }

#region Public Utils

    public void ApplyEffects(StatusEffectData data, object applier, bool appliedByPlayer, int stacksToAdd = 1)
    {
        if(data == null) return;

        if (activeEffects.TryGetValue(data, out var existing))
        {
            existing.remainingDuration = data.duration;

            if (data.stackMode == StatusStackMode.Intensity)
            {
                int prevStacks = existing.stacks;
                existing.stacks = Mathf.Min(existing.stacks + stacksToAdd, data.maxStacks);

                if (existing.stacks > prevStacks)
                {
                    ApplyStatModifierStacks(data, existing, existing.stacks - prevStacks);
                    data.OnStack(existing, gameObject);
                }
            }

            // Refresh duration happens automatically

            OnEffectsChanged?.Invoke();
            return;
        }

        var instance = new ActiveStatusEffect(data, applier, appliedByPlayer);
        activeEffects[data] = instance;

        ApplyStatModifierStacks(data, instance, instance.stacks);
        data.OnApply(instance, gameObject);

        OnEffectsChanged?.Invoke();
    }

    public void RemoveEffect(StatusEffectData data)
    {
        if (!activeEffects.TryGetValue(data, out var instance)) return;

        stats?.RemoveModifiersFromSource(instance);
        data.OnExpire(instance, gameObject);

        activeEffects.Remove(data);
        OnEffectsChanged?.Invoke();
    }

    public bool HasEffect(StatusEffectData data) => activeEffects.ContainsKey(data);

    public int GetStackCount(StatusEffectData data) => activeEffects.TryGetValue(data, out var instance) ? instance.stacks : 0;

    public void RemoveAllOfType(StatusType type)
    {
        keysSnapshot.Clear();
        keysSnapshot.AddRange(activeEffects.Keys);

        foreach (var data in keysSnapshot)
            if(data.type == type)
                RemoveEffect(data);
    }

    public void ClearAll()
    {
        keysSnapshot.Clear();
        keysSnapshot.AddRange(activeEffects.Keys);

        foreach(var data in keysSnapshot)
            RemoveEffect(data);
    }

#endregion

    private void ApplyStatModifierStacks(StatusEffectData data, ActiveStatusEffect instance, int stackDelta)
    {
        if (stats == null) return;

        foreach (var entry in data.statModifiers)
        {
            for (int i = 0; i < stackDelta; i++)
            {
                var mod = new StatModifier(entry.value, entry.modifierType, instance);
                stats.ApplyStatModifier(entry.statID, mod);
            }
        }
    }

    private void TickEffect(StatusEffectData data, ActiveStatusEffect instance)
    {
        data.OnTick(instance, gameObject);

        if (!data.dealsDamageOverTime || damageable == null || !damageable.IsAlive) return;

        float amount = data.amountPerTickPerStack * instance.stacks;
        if (Mathf.Approximately(amount, 0f)) return;

        var hit = new HitData(Mathf.Abs(amount), AttackType.DamageOverTime, instance.appliedByPlayer)
        {
            sourcePos = transform.position,
            isParryable = false,
            isBlockable = false,
        };


        damageable.RecieveHit(hit);
    }


}
