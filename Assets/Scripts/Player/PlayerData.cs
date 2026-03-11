using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player/NewPlayerData", order = 1)]
public class PlayerData : ScriptableObject
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

}
