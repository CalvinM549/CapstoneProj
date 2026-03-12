using System;
using UnityEngine;

public class WeaponData : ScriptableObject
{
    [Header("Light Attacks")]
    public HitInfo[] lightAttacks;

    [Header("Heavy Attack")]
    public HitInfo heavyAttack;

    [Header("Dash Attack")]
    public HitInfo dashAttack;

    [Header("Hitstop")]
    public float hitstopTimescale;

    [Header("Knockback")]

    [Header("Other")]
    public float comboWindow;
    public int maxComboSteps;

    public float dashCancelWindow;
}

[Serializable]
public class HitInfo
{
    public AttackType type;

    public float damage;
    public float startupTime;
    public float activeTime;
    public float recoveryTime;

    public float hitstopDuration;
    public float knockback;
}
