using UnityEngine;

public abstract class AttackExecutorBase : MonoBehaviour, IAttackExecutor
{
    [SerializeField] protected EnemyAttackData attackData;

    protected EnemyController controller;
    protected Animator animator;

    public bool Executing { get; protected set; }
    public EnemyAttackData AttackData => attackData;

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        animator = GetComponentInChildren<Animator>();
    }

    public virtual bool CanExecute(EnemyAIController ai) => ai.context.distanceToTarget <= attackData.attackRange;
    public abstract void BeginTelegraph(EnemyAIController ai);
    public abstract void Execute(EnemyAIController ai);
    public abstract void Interrupt(EnemyAIController ai);

    public virtual void ResetForPool() { }

}
