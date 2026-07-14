using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTargeting : MonoBehaviour
{
    [SerializeField] private float lockRange; // replace with SO
    [SerializeField] private float lockAcquireArea;
    [SerializeField] private LayerMask enemyLayer;

    private Player p;
    private EnemyBase lockedTarget;
    private int lockStacks;


    private InputReader inputs;

    public bool HasTarget => lockedTarget != null && lockedTarget.IsAlive;
    public Transform LockedTarget => lockedTarget?.transform;

    private void Awake()
    {
        p = GetComponent<Player>();
    }

    private void OnEnable()
    {
        inputs = InputManager.Instance.PlayerInputs;

        inputs.TargetPressed += HandleLockInput;
    }

    private void OnDisable()
    {
        inputs.TargetPressed -= HandleLockInput;
    }

    private void Update()
    {
        ValidateLock();
    }

    private void HandleLockInput()
    {
        if (HasTarget)
            CycleLock();
        else
            AcquireLock();
    }

    private void AcquireLock()
    {
        EnemyBase nearest = FindNearestEnemy();
        if (nearest == null) return;

        lockedTarget = nearest;
        GameEvents.LockAcquired(lockedTarget);
    }

    private void CycleLock()
    {
        EnemyBase next = FindNearestEnemyExcluding(lockedTarget);
        if (next == null)
        {
            DropLock();
            return;
        }

        lockedTarget = next;
        GameEvents.LockAcquired(lockedTarget);
    }

    public void DropLock()
    {
        lockedTarget = null;
        GameEvents.LockDropped();
    }

    private void ValidateLock()
    {
        if (!HasTarget) return;

        float dist = Vector2.Distance(transform.position, lockedTarget.transform.position);
        if (dist > lockRange * 1.2f)
            DropLock();
    }

    private EnemyBase FindNearestEnemy() => FindNearestEnemyExcluding(null);

    private EnemyBase FindNearestEnemyExcluding(EnemyBase exclude)
    {
        Vector2 lockPos = p.GetMouseWorldPos();
        Collider2D[] hits = Physics2D.OverlapCircleAll(lockPos, lockAcquireArea, enemyLayer);

        EnemyBase best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            EnemyBase e = hit.GetComponentInParent<EnemyBase>();
            if (e == null || !e.IsAlive || e == exclude) continue;

            float d = Vector2.Distance(lockPos, e.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = e;
            }   
        }

        return best;
    }
}
