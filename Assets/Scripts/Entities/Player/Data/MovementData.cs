using UnityEngine;

[CreateAssetMenu(fileName = "MovementData", menuName = "Player/NewMovementData", order = 1)]
public class MovementData : ScriptableObject
{
    [Header("Inputs")]
    public float inputBufferWindow;

    [Header("Base Movement")]
    public float baseSpeed;
    public float maxSpeed;
    public float acceleration;
    public float deceleration;
    public float turnMultiplier;

    [Space]
    public float maxSpeedBonus;
    public float speedBonusDecayRate;
    [Range(0, 1)] public float dashSpeedRetention;

    [Header("Dash Movement")]
    public float dashSpeed;
    public float dashDuration;
    
    public float dashExitMultiplier;
    
    [Range(0, 1)] public float dashMomentumRetention;
    public float dashMomentumBonusSpeed;

    public int maxDashCharges;
    public float dashRechargeTime;

    public float dashIFrameDuration;

    [Header("Combat Movement")]
    public float attackDecelerationMult;
    public float recoverySpeedMult;

    [Header("Other Movement")]
    public float knockbackMultiplier;
    [Tooltip("Amount of the push retained on end")]
    public float pushExitMultiplier;
}
