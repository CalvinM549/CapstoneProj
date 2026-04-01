using UnityEngine;

[CreateAssetMenu(fileName = "MovementData", menuName = "Player/NewMovementData", order = 1)]
public class MovementData : ScriptableObject
{
    [Header("Base Movement")]
    public float baseSpeed;
    public float acceleration;
    public float deceleration;
    public float turnMultiplier;

    [Header("Dash Movement")]
    public float dashSpeed;
    public float dashDuration;
    public float dashExitMultiplier;
    public int maxDashCharges;
    public float dashRechargeTime;

    public float dashIFrameDuration;

    [Header("Other Movement")]
    public float knockbackMultiplier;
    [Tooltip("Amount of the push retained on end")]
    public float pushExitMultiplier;
}
