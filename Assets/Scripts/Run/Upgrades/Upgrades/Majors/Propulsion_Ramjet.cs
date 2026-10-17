using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Dashing through enemies rams them: each enemy hit once per dash takes damage, knockback and stagger.
// Counts as a Heavy melee hit, so it gains momentum and procs heavy-attack upgrades (e.g. Sunder).
[CreateAssetMenu(menuName = "Upgrades/Major/Propulsion/Ramjet")]
public class Propulsion_Ramjet : MajorUpgrade
{
    [SerializeField] private float radius = 0.8f;
    [SerializeField] private float damage = 12f;
    [SerializeField] private float knockback = 10f;
    [SerializeField] private float hitstun = 0.4f;

    private readonly List<EnemyController> buffer = new();
    private readonly HashSet<EnemyController> hitThisDash = new();
    private Coroutine ramRoutine;

    protected override void OnApply(Player player)
    {
        base.OnApply(player);
        GameEvents.OnPlayerDashStart += HandleDashStart;
    }

    protected override void OnRemove(Player player)
    {
        if (ramRoutine != null && player != null) player.StopCoroutine(ramRoutine);
        ramRoutine = null;

        base.OnRemove(player);
        GameEvents.OnPlayerDashStart -= HandleDashStart;
    }

    private void HandleDashStart()
    {
        if (player == null) return;

        if (ramRoutine != null) player.StopCoroutine(ramRoutine);
        ramRoutine = player.StartCoroutine(RamRoutine());
    }

    private IEnumerator RamRoutine()
    {
        hitThisDash.Clear();

        while (player != null && player.Movement.IsDashing && AIManager.Instance != null)
        {
            Vector2 pos = player.transform.position;
            AIManager.Instance.GetEnemiesInRadius(pos, radius, buffer);

            foreach (var enemy in buffer)
            {
                if (!hitThisDash.Add(enemy)) continue;

                Vector2 dir = (Vector2)enemy.transform.position - pos;
                dir = dir.sqrMagnitude > 0.001f ? dir.normalized : player.Movement.LastMoveDirection;

                var hit = BuildPlayerMeleeHit(damage, AttackType.Heavy, dir, knockback, hitstun);
                GameEvents.HitConfirmed(hit);
                enemy.RecieveHit(hit);

                if (VFXManager.Instance != null) VFXManager.Instance.PlayVFX(VFXType.HitCross, enemy.transform.position);
            }

            yield return null;
        }

        ramRoutine = null;
    }
}
