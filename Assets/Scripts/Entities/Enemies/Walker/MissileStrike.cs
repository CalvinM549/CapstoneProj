using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileStrike : MonoBehaviour
{
    [SerializeField] private SimpleTelegraph telegraph;
    [SerializeField] private Transform missileVisual;

    [SerializeField] private float spawnHeight = 12f;

    private Vector2 targetPos;
    private HitData hitData;
    private float radius;
    private float warningDelay;
    private float fallDuration;

    public void Initialize(Vector2 target, HitData hitData, float radius, float warningDelay, float fallDuration)
    {
        this.targetPos = target;
        this.hitData = hitData;
        this.radius = radius;
        this.warningDelay = warningDelay;
        this.fallDuration = fallDuration;

        transform.position = target;

        if(missileVisual != null)
            missileVisual.gameObject.SetActive(false);

        telegraph.Play(target, radius, warningDelay + fallDuration);

        StartCoroutine(StrikeRoutine());
    }

    private IEnumerator StrikeRoutine()
    {
        yield return new WaitForSeconds(warningDelay);

        DropMissile();
        yield return new WaitForSeconds(fallDuration);

        Detonate();
    }

    private void DropMissile()
    {
        if (missileVisual == null) return;

        missileVisual.position = targetPos + Vector2.up * spawnHeight;
        missileVisual.gameObject.SetActive(true);
        missileVisual
            .DOMoveY(targetPos.y, fallDuration)
            .SetEase(Ease.Linear)
            .SetLink(gameObject);
    }

    private void Detonate()
    {
        VFXManager.Instance.PlayVFX(VFXType.ExplosionComplex, targetPos);

        var hits = Physics2D.OverlapCircleAll(transform.position, radius, GameManager.PlayerLayer);
        var alreadyHit = new HashSet<IDamageable>();

        foreach (var col in hits)
        {
            if (!col.TryGetComponent<IDamageable>(out var target)) continue;
            if (!alreadyHit.Add(target)) continue;


            Vector2 dir = ((Vector2)col.transform.position - targetPos).normalized;

            HitData hit = hitData.Clone();
            hit.sourcePos = targetPos;
            hit.knockbackDirection = dir;

            target.RecieveHit(hit);

            CameraManager.Instance.CameraShake(2f);
        }

        if (alreadyHit.Count == 0)
            CameraManager.Instance.CameraShake(0.7f);

        Destroy(gameObject);
    }

}
