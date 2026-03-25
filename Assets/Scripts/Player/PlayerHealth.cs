using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerMovement movement;

    private SpriteRenderer sr;

    public bool IsAlive { get; set; }
    public bool IsIframe { get; private set; }
    public bool IsHitstunned { get; private set; }

    private int currentHealth;
    private int maxHealth;

    private Coroutine hitStunRoutine;
    private Coroutine iFrameRoutine;

    public float PercentHealth => currentHealth / maxHealth;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void RecieveHit(HitData hit)
    {
        if (!IsAlive) return;
        if (IsIframe) return;

        if (!combat.HandlePlayerHit(hit)) return;

        ApplyDamage(hit);
    }

    private void ApplyDamage(HitData hit)
    {
        currentHealth = Mathf.Max(0, currentHealth - hit.damage);

        GameEvents.PlayerHit(hit);
        GameEvents.PlayerHealthChanged(currentHealth, maxHealth);

        if (currentHealth <= 0)
            PlayerDeath();
    }


    public void GrantIFrames(float duration)
    {
        if (iFrameRoutine != null)
            StopCoroutine(iFrameRoutine);

        iFrameRoutine = StartCoroutine(IFrameRoutine(duration));
    }

    private IEnumerator IFrameRoutine(float duration)
    {
        IsIframe = true;
        sr.color = Color.red;

        yield return new WaitForSeconds(duration);

        sr.color = Color.white;

        if (IsAlive)
        {
            IsIframe = false;
        }
    }

    private IEnumerator HitStunRoutine()
    {
        float duration = 0.1f;

        yield return new WaitForSeconds(duration);
    }

    private void PlayerDeath()
    {
        IsAlive = false;
        IsIframe = true;

        StopAllCoroutines();

        GameEvents.PlayerDeath();

    }
}
