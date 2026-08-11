using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    public EnemyContext context;
    public AIProfile profile;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform self;

    public List<IAttackExecutor> attackExecutors = new();

    private IEnemyState currentState;

    public Rigidbody2D Body => rb;
    public Transform Self => self;
    [NonSerialized] public float nextTickTime;

    private void Awake()
    {
        GetComponents<IAttackExecutor>(attackExecutors);
    }

    public void ResetController(AIProfile newProfile)
    {
        profile = newProfile;
        context = default;
        currentState = null;
        ChangeState(EnemyStates.Idle);
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState?.Enter(this);
    }

    public void Tick(float dt)
    {
        currentState?.Tick(this, dt);
    }
}

public class EnemyContext
{
    public Transform target;
    public Vector2 targetLastKnownPos;
    public float distanceToTarget;

    public float timeSinceAttack;
    public int attackPatternIndex;
    public int activeExecutorIndex;

    public float staggerTimer;
    public bool hasLineOfSight;
}