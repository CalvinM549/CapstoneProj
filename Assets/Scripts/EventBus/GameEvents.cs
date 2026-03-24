using System;
using UnityEngine;

public static class GameEvents
{

    #region Momentum
    public static event Action<float> OnMomentumChange;
    public static event Action<MomentumZone, MomentumZone> OnMomentumZoneChange; // Old momentum, new momentum

    public static event Action OnEnterHighMomentum;
    public static event Action OnEnterMidMomentum;
    public static event Action OnEnterLowMomentum;
    public static event Action OnEnterEmptyMomentum;

    public static void MomentumChange(float normalized) => OnMomentumChange?.Invoke(normalized);
    public static void MomentumZoneChange(MomentumZone _old, MomentumZone _new) => OnMomentumZoneChange?.Invoke(_old, _new);

    public static void EnterHighMomentum() => OnEnterHighMomentum?.Invoke();
    public static void EnterMidMomentum() => OnEnterMidMomentum?.Invoke();
    public static void EnterLowMomentum() => OnEnterLowMomentum?.Invoke();
    public static void EnterEmptyMomentum() => OnEnterEmptyMomentum?.Invoke();

    #endregion

    #region Combat
    public static event Action<AttackType> OnAttackStarted;
    public static event Action<AttackType> OnAttackEnded;
    public static event Action<HitData> OnHitConfirmed;

    public static event Action OnHeavyAttackWhiff;

    public static void AttackStarted(AttackType type) => OnAttackStarted?.Invoke(type);
    public static void AttackEnded(AttackType type) => OnAttackEnded?.Invoke(type);
    public static void HitConfirmed(HitData hit) => OnHitConfirmed?.Invoke(hit);
    public static void HeavyAttackWhiff() => OnHeavyAttackWhiff?.Invoke();

    #endregion

    #region Player
    public static event Action<HitData> OnPlayerHit;
    public static event Action OnPlayerDeath;
    public static event Action<int, int> OnPlayerHealthChange;

    public static void PlayerHit(HitData hit) => OnPlayerHit?.Invoke(hit);
    public static void PlayerHealthChanged(int currentHealth, int maxHealth) => OnPlayerHealthChange?.Invoke(currentHealth, maxHealth);
    public static void PlayerDeath() => OnPlayerDeath?.Invoke();

    #endregion

    #region Enemy

    public static event Action<EnemyBase, HitData> OnEnemyHit;
    public static event Action<EnemyBase> OnEnemyKilled;

    public static void EnemyHit(EnemyBase enemy, HitData hit) => OnEnemyHit?.Invoke(enemy, hit);
    public static void EnemyKilled(EnemyBase enemy) => OnEnemyKilled?.Invoke(enemy);

    #endregion






    // Heat

    // Draft

    // Upgrades
}
