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
    [Tooltip("Time after full combo, before player can attack again")]
    public float comboCooldown;
    public int maxComboSteps;

    public float heavyHoldThreshold;

    public float dashExtraWindow;
}


