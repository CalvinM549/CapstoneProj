using UnityEngine;

public static class EnemyStates
{
    public static readonly IdleState Idle = new();
    public static readonly ChaseState Chase = new();
    public static readonly AttackState Attack = new();
    public static readonly RepositionState Reposition = new();
    public static readonly StaggeredState Staggered = new();

}

public interface IEnemyState
{
    void Enter(EnemyAIController ai);
    void Tick(EnemyAIController ai, float dt);
    void Exit(EnemyAIController ai);
}

public class IdleState : IEnemyState
{
    public void Enter(EnemyAIController ai)
    {
        ai.Body.linearVelocity = Vector2.zero;
    }

    public void Tick(EnemyAIController ai, float dt)
    {

        if (AIManager.Instance.TryFindTarget(ai, out var target))
        {
            ai.context.target = target;
            ai.ChangeState(EnemyStates.Chase);
        }
    }
    public void Exit(EnemyAIController ai)
    {
    }
}

public class ChaseState : IEnemyState
{
    public void Enter(EnemyAIController ai)
    {
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        ref var ctx = ref ai.context;
        if (ctx.target == null)
        {
            ai.ChangeState(EnemyStates.Idle);
            return;
        }

        Vector2 toTarget = (Vector2)ctx.target.position - ai.Body.position;
        ctx.distanceToTarget = toTarget.magnitude;

        if (ctx.distanceToTarget < ai.profile.attackRange)
        {
            ai.ChangeState(EnemyStates.Attack);
            return;
        }

        Vector2 dir = toTarget.normalized;
        ai.Body.linearVelocity = dir * ai.profile.moveSpeed;
    }

    public void Exit(EnemyAIController ai)
    {
        ai.Body.linearVelocity = Vector2.zero;
    }
}

public class AttackState : IEnemyState
{
    public void Enter(EnemyAIController ai)
    {
        var ctx = ai.context;
        var executor = SelectExecutor(ai);

        if (executor == null)
        {
            ai.ChangeState(EnemyStates.Reposition);
            return;
        }

        ctx.activeExecutorIndex = ai.attackExecutors.IndexOf(executor);
        ctx.timeSinceAttack = 0f;
        ai.Body.linearVelocity = Vector2.zero;
        executor.BeginTelegraph(ai);
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        var ctx = ai.context;
        var executor = ai.attackExecutors[ctx.activeExecutorIndex];
        ctx.timeSinceAttack += dt;

        if (ctx.timeSinceAttack >= executor.TelegraphDuration)
        {
            executor.Execute(ai);
            ai.ChangeState(EnemyStates.Reposition);
        }
    }

    public void Exit(EnemyAIController ai)
    {
        var executor = ai.attackExecutors[ai.context.activeExecutorIndex];
        executor.CancelTelegraph(ai);
    }

    private IAttackExecutor SelectExecutor(EnemyAIController ai)
    {
        ref var ctx = ref ai.context;
        var pattern = ai.profile.attackPattern;
        var executor = ai.attackExecutors[pattern[ctx.attackPatternIndex % pattern.Length]];
        ctx.attackPatternIndex++;
        return executor.CanExecute(ai) ? executor : FallbackToInRange(ai);
    }

    private IAttackExecutor FallbackToInRange(EnemyAIController ai)
    {
        ref var ctx = ref ai.context;
        for (int i = 0; i < ai.attackExecutors.Count; i++)
        {
            var executor = ai.attackExecutors[i];
            if (!executor.CanExecute(ai)) continue;
            else return executor;
        }

        return null;
    }
}

public class RepositionState : IEnemyState
{
    public void Enter(EnemyAIController ai)
    {
        throw new System.NotImplementedException();
    }

    public void Exit(EnemyAIController ai)
    {
        throw new System.NotImplementedException();
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        throw new System.NotImplementedException();
    }
}

public class StaggeredState : IEnemyState
{
    public void Enter(EnemyAIController ai)
    {
        ai.context.staggerTimer = ai.profile.staggerDuration;
        ai.Body.linearVelocity = Vector2.zero;
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        ai.context.staggerTimer -= dt;
        if (ai.context.staggerTimer <= 0f)
            ai.ChangeState(ai.context.target != null ? EnemyStates.Chase : EnemyStates.Idle);
    }

    public void Exit(EnemyAIController ai)
    { }

}