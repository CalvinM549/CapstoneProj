using UnityEngine;

public static class StatRef
{
    #region PlayerStats

    // Movement
    public static string PlayerBaseSpeed = "player.movement.baseSpeed";
    public static string PlayerDashCharges = "player.movement.dashCharges";
    public static string PlayerDashRecharge = "player.movement.dashRechargeRate";

    // Combat
    public static string PlayerBaseMeleeDamage = "player.combat.meleeDamage";
    public static string PlayerBaseRangedDamage = "player.combat.rangedDamage";
    public static string PlayerBaseGlobalDamage = "player.combat.globalDamage";

    // Health
    public static string PlayerStructureCount = "player.health.structureCount";
    public static string PlayerBaseStructureHealth = "player_baseStructureHealth";

    // Momentum
    public static string PlayerMomentumGainMult = "player.momentum.gainMult";
    public static string PlayerMomentumDrainMult = "player.momentum.drainMult";

    // Heat
    public static string PlayerHeatRate = "player_baseHeatRate";


    #endregion
}
