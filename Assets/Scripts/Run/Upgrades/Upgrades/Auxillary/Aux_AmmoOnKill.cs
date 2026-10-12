using UnityEngine;

public class Aux_AmmoOnKill : AuxUpgrade
{
    [SerializeField] private float chanceForAmoPerStack = 0.5f;

    protected override void OnApply(Player player)
    {
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
    }

    protected override void OnRemove(Player player)
    {
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void HandleEnemyKilled(EnemyController enemy)
    {
        float roll = Random.Range(0f, 1f);
        if (roll < (chanceForAmoPerStack * currentStacks))
        {
            player.Combat.RestoreAmmo(1);
        }

    }
}
