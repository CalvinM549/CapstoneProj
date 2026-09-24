using UnityEngine;

[CreateAssetMenu(menuName = "Status Effects/Lightning")]
public class LightningStatusEffect : StatusEffectData
{
    [SerializeField] private float damagePerStack;

    public override void OnExpire(ActiveStatusEffect instance, GameObject target)
    {
        if (!target.TryGetComponent<IDamageable>(out var damageable)) return;

        bool isPlayer = target.CompareTag("Player");

        HitData hit = new HitData(damagePerStack * instance.stacks, AttackType.Elemental, !isPlayer)
        {
            knockbackForce = 0f,

            isParryable = false,
            isBlockable = false,
        };

        damageable.RecieveHit(hit);

        VFXManager.Instance.PlayVFX(VFXType.LightningStrike, target.transform.position);
    }
}
