using UnityEngine;

public abstract class AttackExecutorBase : MonoBehaviour, IAttackExecutor
{
    [SerializeField] protected float telegraphDuration;
    [SerializeField] protected float range;

    public float TelegraphDuration => telegraphDuration;

    public virtual bool CanExecute(EnemyAIController ai) => ai.context.distanceToTarget <= range;

    public abstract void BeginTelegraph(EnemyAIController ai);
    public abstract void Execute(EnemyAIController ai);
    public abstract void CancelTelegraph(EnemyAIController ai);

    public virtual void ResetForPool() { }

}
