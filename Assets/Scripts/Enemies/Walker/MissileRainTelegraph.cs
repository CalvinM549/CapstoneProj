using DG.Tweening;
using System.Collections;
using UnityEngine;

public class MissileRainTelegraph : MonoBehaviour
{
    private ProjectileData data;
    [SerializeField] private SpriteRenderer indicatorSprite;

    [SerializeField] private Transform missileVisual;
    [SerializeField] private GameObject explosionVFXPrefab;

    private Vector2 targetPosition;
    [SerializeField] private float radius;
    [SerializeField] private float timeToDetonate;

    public void Initialize(Vector2 position, ProjectileData data)
    {
        this.data = data;
        targetPosition = position;
        transform.position = position;

        if (indicatorSprite != null)
        {
            float diameter = radius * 2f;
            indicatorSprite.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        StartCoroutine(TelegraphRoutine());
    }

    private IEnumerator TelegraphRoutine()
    {
        yield return new WaitForSeconds(1f);

        missileVisual.gameObject.SetActive(true);

        missileVisual.DOMoveY(0f, timeToDetonate).SetEase(Ease.Linear);


        yield return new WaitForSeconds(timeToDetonate);
        
        Detonate();
    }

    private void Detonate()
    {
        if (explosionVFXPrefab != null)
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPosition, radius);

        foreach (Collider2D hit in hits)
        {
            print(hit.gameObject.name);

            if (!hit.CompareTag("PlayerHurtbox") && !hit.CompareTag("Player")) continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive) continue;
            
            Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;

            var hitData = new HitData()
            {
                // Setup hit properly
                damage = data.damage,
                damageType = data.damageType,

                sourcePos = transform.position,
                knockbackDirection = knockbackDir,
                knockbackForce = data.knockback,

                hitstopTime = data.hitstopDuration,
                hitstunTime = data.hitstunTime,

                isPlayerAttack = false,
                isParryable = false,

            };

            // Ping event?

            target.RecieveHit(hitData);
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (data == null) return;
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif

}
