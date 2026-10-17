using System.Collections.Generic;
using UnityEngine;

// Killing a Shocked enemy grounds its charge into everything nearby: its Shock spreads to enemies in range.
[CreateAssetMenu(menuName = "Upgrades/Aux/Grounding Spike")]
public class Aux_GroundingSpike : AuxUpgrade
{
    [SerializeField] private StatusEffectData shockStatus;
    [SerializeField] private float radius = 4f;
    [Tooltip("Extra radius per additional stack.")]
    [SerializeField] private float radiusPerStack = 1f;

    private readonly List<EnemyController> buffer = new();

    protected override void OnApply(Player player) => GameEvents.OnEnemyKilled += HandleEnemyKilled;
    protected override void OnRemove(Player player) => GameEvents.OnEnemyKilled -= HandleEnemyKilled;

    private void HandleEnemyKilled(EnemyController victim)
    {
        if (shockStatus == null || AIManager.Instance == null) return;

        int stacks = 0;
        foreach (var status in victim.StatusesAtDeath)
            if (status.data == shockStatus) stacks = status.stacks;

        if (stacks <= 0) return;

        Vector2 center = victim.transform.position;
        AIManager.Instance.GetEnemiesInRadius(center, radius + radiusPerStack * (currentStacks - 1), buffer);

        foreach (var enemy in buffer)
        {
            if (enemy.Statuses == null) continue;
            enemy.Statuses.ApplyEffects(shockStatus, this, true, stacks);
            if (VFXManager.Instance != null) VFXManager.Instance.PlayVFX(VFXType.LightningStrike, enemy.transform.position);
        }
    }
}
