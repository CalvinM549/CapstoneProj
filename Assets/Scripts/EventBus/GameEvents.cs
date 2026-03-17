using System;
using UnityEngine;

public static class GameEvents
{
    // Momentum System
    public static event Action<float> OnMomentumChange;
    public static event Action<MomentumZone, MomentumZone> OnMomentumZoneChange; // Old momentum, new momentum

    public static event Action OnEnterHighMomentum;
    public static event Action OnEnterMidMomentum;
    public static event Action OnEnterLowMomentum;
    public static event Action OnEnterEmptyMomentum;

    // Combat

    // Attacking
    public static event Action LightAttackStart;

    // Player Health
    public static event Action<HitData> OnPlayerHit;
    public static event Action<int, int> OnPlayerHealthChange;
    public static event Action OnPlayerDeath;
    

    public static event Action<HitData> OnHitConfirmed;

    // Heat

    // Draft

    // Upgrades


    // Event Firers

    public static void MomentumChange(float normalized) => OnMomentumChange?.Invoke(normalized);
    public static void MomentumZoneChange(MomentumZone old, MomentumZone _new) => OnMomentumZoneChange?.Invoke(old, _new);

    public static void EnterHighMomentum() => OnEnterHighMomentum?.Invoke();
    public static void EnterMidMomentum() => OnEnterMidMomentum?.Invoke();
    public static void EnterLowMomentum() => OnEnterLowMomentum?.Invoke();
    public static void EnterEmptyMomentum() => OnEnterEmptyMomentum?.Invoke();


    public static void PlayerHit(HitData hit)
    {
        OnPlayerHit?.Invoke(hit);
    }

    public static void PlayerHealthChanged(int currentHealth, int maxHealth)
    {
        OnPlayerHealthChange?.Invoke(currentHealth, maxHealth);
    }

    public static void PlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }

    public static void HitConfirmed(HitData hit)
    {
        OnHitConfirmed?.Invoke(hit);
    }
}
