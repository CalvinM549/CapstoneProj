using DG.Tweening;
using System.Collections;
using UnityEngine;

public class MissileRainTelegraph : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;

    [SerializeField] private Transform missileVisual;
    [SerializeField] private GameObject explosionVFXPrefab;

    private Vector2 targetPosition;
    private float radius;
    private float timeToDetonate;

    public void Initialize(Vector2 position)
    {
        targetPosition = position;
        transform.position = position;

        if (sr != null)
        {
            // Scale with aoe data
        }

        StartCoroutine(TelegraphRoutine());
    }

    private IEnumerator TelegraphRoutine()
    {
        missileVisual.gameObject.SetActive(true);

        missileVisual.DOMoveY(0f, timeToDetonate);


        yield return new WaitForSeconds(timeToDetonate);
        
        Detonate();
    }

    private void Detonate()
    {
        if (explosionVFXPrefab != null)
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(targetPosition, radius);

        bool hitPlayer = false;

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("PlayerHurtbox") && !hit.CompareTag("Player")) continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target == null || !target.IsAlive) continue;
            
            Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;

            var hitData = new HitData()
            {
                // Setup hit properly

                sourcePos = transform.position,
                knockbackDirection = knockbackDir,
                isPlayerAttack = false,
                isParryable = false,
                
            };

            // Ping event

            target.RecieveHit(hitData);
            hitPlayer = true;
        }

        Destroy(gameObject);
    }
}
