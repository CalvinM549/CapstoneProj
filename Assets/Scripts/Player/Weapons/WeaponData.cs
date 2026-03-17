using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Player/NewWeaponData", order = 2)]
public class WeaponData : ScriptableObject
{
    [Header("Light Attacks")]
    public AttackInfo[] lightAttacks;

    [Header("Heavy Attack")]
    public AttackInfo heavyAttack;

    [Header("Dash Attack")]
    public AttackInfo dashAttack;

    [Header("Other")]
    public float comboWindow;
    public int maxComboSteps;

    public float dashCancelWindow;
}

[Serializable]
public class AttackInfo
{
    public AttackType type;

    public int damage;
    public float startupTime;
    public float activeTime;
    public float recoveryTime;

    public float hitstopDuration;
    public float knockback;
}
