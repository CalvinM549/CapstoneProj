using UnityEngine;

public static class StatRef
{
    #region PlayerStats

    // Movement
    public static string PlayerBaseSpeed = "player.movement.baseSpeed";
    public static string PlayerDashCharges = "player.movement.dashCharges";
    public static string PlayerDashRecharge = "player.movement.dashRechargeRate";

    // Combat
    public static string PlayerMeleeDamageMult = "player.combat.meleeDamage";
    public static string PlayerRangedDamageMult = "player.combat.rangedDamage";

    public static string PlayerOutgoingDamageMult = "player.combat.globalDamage";
    public static string PlayerIncomingDamageMult = "player.combat.incomingDamageMult";

    // Health
    public static string PlayerStructureCount = "player.health.structureCount";
    public static string PlayerBaseStructureHealth = "player_baseStructureHealth";

    // Momentum
    public static string PlayerMomentumGainMult = "player.momentum.gainMult";
    public static string PlayerMomentumDrainMult = "player.momentum.drainMult";

    // Heat
    public static string PlayerHeatRate = "player_baseHeatRate";


    #endregion

    #region EnemyStats

    public static string EnemyBaseSpeed = "enemy.movement.baseSpeed";


    public static string EnemyOutgoingDamageMult = "enemy.combat.outgoingDamageMult";
    public static string EnemyIncomingDamageMult = "enemy.combat.incomingDamageMult";

    public static string EnemyMaxHealth = "enemy.health.maxHealth";

    #endregion
}
