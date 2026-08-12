using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyAIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stateDisplay;

    public EnemyContext context;
    [HideInInspector] public AIProfile profile;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform self;

    [NonSerialized] public List<IAttackExecutor> attackExecutors = new();

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
        context = new();
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
        currentState?.Tick(this, dt);
    }

    public int GetIndex(IAttackExecutor executor) => attackExecutors.IndexOf(executor);

    public IAttackExecutor GetExecutor(int index) => attackExecutors[index] ?? null;

    public IAttackExecutor GetValidExecutor()
    {
        for (int i = 0; i < attackExecutors.Count; i++)
        {
            var executor = attackExecutors[i];
            if (!executor.CanExecute(this)) continue;
            else return executor;
        }

        return null;
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
}