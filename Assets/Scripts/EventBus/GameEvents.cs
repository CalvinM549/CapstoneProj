using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{

    #region Inputs



    #endregion

    #region Momentum
    public static event Action<float> OnMomentumChange;
    public static event Action<MomentumZone, MomentumZone> OnMomentumZoneChange; // Old momentum, new momentum
    public static event Action OnPassiveDrainStart;
    public static event Action OnPassiveDrainEnd;

    public static event Action OnEnterHighMomentum;
    public static event Action OnEnterMidMomentum;
    public static event Action OnEnterLowMomentum;
    public static event Action OnEnterEmptyMomentum;

    public static void MomentumChange(float normalized) => OnMomentumChange?.Invoke(normalized);
    public static void MomentumZoneChange(MomentumZone _old, MomentumZone _new) => OnMomentumZoneChange?.Invoke(_old, _new);

    public static void PassiveDrainStart() => OnPassiveDrainStart?.Invoke();
    public static void PassiveDrainEnd() => OnPassiveDrainEnd?.Invoke();

    public static void EnterHighMomentum() => OnEnterHighMomentum?.Invoke();
    public static void EnterMidMomentum() => OnEnterMidMomentum?.Invoke();
    public static void EnterLowMomentum() => OnEnterLowMomentum?.Invoke();
    public static void EnterEmptyMomentum() => OnEnterEmptyMomentum?.Invoke();

    #endregion

    #region Combat
    public static event Action<AttackType, Vector2> OnAttackStarted; // Player attacks only
    public static event Action<AttackType> OnAttackEnded; // Player Attacks only
    public static event Action<HitData> OnHitConfirmed;

    public static event Action<AttackType> OnRecoveryCancel;
    public static event Action<AttackType> OnAttackWhiff;

    public static void AttackStarted(AttackType type, Vector2 direction) => OnAttackStarted?.Invoke(type, direction);
    public static void AttackEnded(AttackType type) => OnAttackEnded?.Invoke(type);
    public static void HitConfirmed(HitData hit) => OnHitConfirmed?.Invoke(hit);
    public static void RecoveryCancel(AttackType type) => OnRecoveryCancel?.Invoke(type);
    public static void AttackWhiff(AttackType type) => OnAttackWhiff?.Invoke(type);

    #endregion

    #region Player
    public static event Action<HitData> OnPlayerHit;
    public static event Action OnPlayerDeath;
    public static event Action OnPlayerDashStart;
    public static event Action OnPlayerDashEnd;
    public static event Action<List<HealthSegment>> OnPlayerHealthChanged;
    //public static event Action<int, int> OnPlayerHealthChange;
    public static event Action<float[]> OnDashChargeChange;

    public static void DashChargeChange(float[] rechargeStates) => OnDashChargeChange?.Invoke(rechargeStates);
    public static void PlayerHit(HitData hit) => OnPlayerHit?.Invoke(hit);
    public static void PlayerHealthChanged(List<HealthSegment> newHealth) => OnPlayerHealthChanged?.Invoke(newHealth);
    //public static void PlayerHealthChanged(int currentHealth, int maxHealth) => OnPlayerHealthChange?.Invoke(currentHealth, maxHealth);
    public static void PlayerDeath() => OnPlayerDeath?.Invoke();
    public static void PlayerDashStart() => OnPlayerDashStart?.Invoke();
    public static void PlayerDashEnd() => OnPlayerDashEnd?.Invoke();

    public static event Action OnPlayerParryStart;
    public static event Action OnPlayerParryEnd;

    public static void PlayerParryStart() => OnPlayerParryStart?.Invoke();
    public static void PlayerParryEnd() => OnPlayerParryEnd?.Invoke();

    #endregion



    #region Enemy

    public static event Action<EnemyBase, HitData> OnEnemyHit;
    public static event Action<EnemyBase> OnEnemyKilled;

    public static void EnemyHit(EnemyBase enemy, HitData hit) => OnEnemyHit?.Invoke(enemy, hit);
    public static void EnemyKilled(EnemyBase enemy) => OnEnemyKilled?.Invoke(enemy);

    #endregion


    //public static event Action<EnemyBase>

    // Heat

    // Draft

    // Upgrades
}
