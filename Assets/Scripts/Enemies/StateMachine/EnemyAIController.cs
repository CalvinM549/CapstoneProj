using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stateDisplay;

    [HideInInspector] public EnemyContext context = new();
    public EnemyStats stats {  get; private set; }
    public StatValue speedStat { get; private set; } // cached for performance

    [HideInInspector] public AIProfile profile;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform self;

    [NonSerialized] public List<IAttackExecutor> attackExecutors = new();
    private readonly List<IAttackExecutor> primaryExecutors = new();

    private readonly List<int> primaryExecutorIndexes = new();


    private IEnemyState currentState;

    public IEnemyState PostAggroState => EnemyStates.MoveIntoRange;
    public IEnemyState PostAttackState => EnemyStates.Reposition;

    public Rigidbody2D Body => rb;
    public Transform Self => self;
    [NonSerialized] public float nextTickTime;

    private void Awake()
    {
        GetComponents<IAttackExecutor>(attackExecutors);
        stats = GetComponent<EnemyStats>();

        primaryExecutors.Clear();
        primaryExecutorIndexes.Clear();

        for (int i = 0; i < attackExecutors.Count; i++)
        {
            if (attackExecutors[i].AttackData.role == AttackRole.Primary)
            {
                primaryExecutors.Add(attackExecutors[i]);
                primaryExecutorIndexes.Add(i);
            }
        }
    }

    public void ResetController(AIProfile newProfile)
    {
        speedStat = stats.GetStatValue(StatRef.EnemyBaseSpeed);

        profile = newProfile;
        context.Reset();
        currentState = null;
        ChangeState(EnemyStates.Idle);
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);

        if(stateDisplay != null)
            stateDisplay.text = newState.ToString();
    }

    public void Tick(float dt)
    {
        RefreshTargetInfo();
        currentState?.Tick(this, dt);
    }

    public Vector2 GetChaseDirection()
    {
        Vector2 from = rb.position;
        Vector2 to = context.target.position;

        Vector2 direct = (to - from).normalized;

        INavigation nav = AIManager.Instance.Nav;
        if (nav == null || nav.HasClearPath(from, to, profile.agentRadius, profile.obstacleLayer))
            return direct;

        return nav.TryGetMoveDirection(context.target, from, out Vector2 dir) ? dir : direct;
    }

    private void RefreshTargetInfo()
    {
        if (context.target == null) return;

        Vector2 toTarget = (Vector2)context.target.position - Body.position;
        context.distanceToTarget = toTarget.magnitude;
        context.targetLastKnownPos = context.target.position;
    }

    public void ForceStagger(float duration)
    {
        context.staggerTimer = duration;
        ChangeState(EnemyStates.Staggered);
    }

    public int GetIndex(IAttackExecutor executor) => attackExecutors.IndexOf(executor);
    public IAttackExecutor GetExecutor(int index) => attackExecutors[index] ?? null;


    public IAttackExecutor GetValidExecutor()
    {
        return GetPatternExecutor() ?? GetBestExecutor(AttackRole.Primary);
    }
    public IAttackExecutor GetPrimaryExecutor() => GetBestExecutor(AttackRole.Primary);
    public IAttackExecutor GetPunishExecutor() => GetBestExecutor(AttackRole.Punish);


    private IAttackExecutor GetPatternExecutor()
    {
        var pattern = profile.attackPattern;

        if (pattern == null || pattern.Length == 0 | primaryExecutors.Count == 0)
            return null;

        int patternSlot = pattern[context.attackPatternIndex % pattern.Length];
        if (patternSlot < 0 || patternSlot >= primaryExecutors.Count)
        {
            Debug.LogWarning($"[EnemyAIController] attackPattern entry {patternSlot} on profile '{profile.name}' is out of range for {primaryExecutors.Count} primary attack(s) on {name}. Falling back to priority selection.");

            return null;
        }

        var desired = primaryExecutors[patternSlot];
        if (!desired.CanExecute(this)) return null;

        context.activeExecutorIndex = primaryExecutorIndexes[patternSlot];
        context.attackPatternIndex++;
        return desired;
    }


    public IAttackExecutor GetBestExecutor(AttackRole role)
    {
        IAttackExecutor best = null;
        int bestIndex = -1;

        int bestPriority = int.MinValue;

        for (int i = 0; i < attackExecutors.Count; i++)
        {
            var executor = attackExecutors[i];

            if(executor.AttackData.role != role) continue;
            if (!executor.CanExecute(this)) continue;

            if (executor.AttackData.priority > bestPriority)
            {
                bestPriority = executor.AttackData.priority;
                best = executor;
                bestIndex = i;
            }
        }

        if(best != null)
            context.activeExecutorIndex = bestIndex;

        return best;
    }



}

[Serializable]
public class EnemyContext
{
    public Transform target;
    public Vector2 targetLastKnownPos;
    public float distanceToTarget;

    public float stateTimer;

    // Attack Executors
    public float timeSinceAttack;
    public int attackPatternIndex;
    public int activeExecutorIndex;
    public float currentAttackRange;

    public float staggerTimer;
    public bool hasLineOfSight;

    public int repositionSign = 1;

    public void Reset()
    {
        target = null;
        targetLastKnownPos = default;
        distanceToTarget = 0f;

        stateTimer = 0f;

        timeSinceAttack = 0f;
        attackPatternIndex = 0;
        activeExecutorIndex = 0;
        currentAttackRange = 0f;

        staggerTimer = 0f;
        hasLineOfSight = false;

        repositionSign = 1;
    }
}