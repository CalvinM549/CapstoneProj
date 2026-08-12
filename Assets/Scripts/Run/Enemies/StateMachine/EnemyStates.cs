using UnityEngine;

public static class EnemyStates
{
    public static readonly IdleState Idle = new();
    public static readonly MoveIntoRangeState MoveIntoRange = new();
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
        // Set idle animation
    }

    public void Tick(EnemyAIController ai, float dt)
    {

        if (AIManager.Instance.TryFindTarget(ai, out var target))
        {
            ai.context.target = target;
            ai.ChangeState(EnemyStates.MoveIntoRange);
        }
    }
    public void Exit(EnemyAIController ai)
    {
    }
}

public class MoveIntoRangeState : IEnemyState
{
    public void Enter(EnemyAIController ai)
    {
        var ctx = ai.context;
        ctx.currentAttackRange = ai.GetValidExecutor().AttackData.attackRange;
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        var ctx = ai.context;

        if (ctx.target == null)
        {
            ai.ChangeState(EnemyStates.Idle);
            return;
        }

        Vector2 toTarget = (Vector2)ctx.target.position - ai.Body.position;
        ctx.distanceToTarget = toTarget.magnitude;

        if (ctx.distanceToTarget < ctx.currentAttackRange && AIManager.Instance.TryFindTarget(ai, out var target))
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
        var executor = ai.GetExecutor(ctx.activeExecutorIndex);

        if (executor == null)
        {
            ai.ChangeState(EnemyStates.Reposition);
            return;
        }

        ctx.timeSinceAttack = 0f;
        ai.Body.linearVelocity = Vector2.zero;
        executor.BeginTelegraph(ai);
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        var ctx = ai.context;
        var executor = ai.GetExecutor(ctx.activeExecutorIndex);
        ctx.timeSinceAttack += dt;

        if (ctx.timeSinceAttack >= executor.AttackData.telegraphDuration && !executor.Executing)
        {
            executor.Execute(ai);
        }

        if (ctx.timeSinceAttack >= executor.AttackData.winddownDuration + executor.AttackData.telegraphDuration && !executor.Executing)
        {
            ai.ChangeState(EnemyStates.Reposition);
        }
    }

    public void Exit(EnemyAIController ai)
    {
        var executor = ai.attackExecutors[ai.context.activeExecutorIndex];
        executor.Interrupt(ai);
    }
}

public class RepositionState : IEnemyState
{
    private const float defaultRepositionDuration = 0.3f;

    public void Enter(EnemyAIController ai)
    {
        ai.context.stateTimer = ai.profile.repositionDuration > 0
            ? ai.profile.repositionDuration
            : defaultRepositionDuration;
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        var ctx = ai.context;

        if (ctx.target == null)
        {
            ai.ChangeState(EnemyStates.Idle);
            return;
        }

        ctx.stateTimer -= dt;

        if (ai.profile.moveSpeed > 0f)
        {
            Vector2 toTarget = (Vector2)ctx.target.position - ai.Body.position;
            Vector2 lateralDir = Vector2.Perpendicular(toTarget.normalized);
            ai.Body.linearVelocity = lateralDir * (ai.profile.moveSpeed);
        }

        if (ctx.stateTimer <= 0f)
            ai.ChangeState(EnemyStates.MoveIntoRange);


    }

    public void Exit(EnemyAIController ai)
    {
        ai.Body.linearVelocity = Vector2.zero;
    }

}

public class StaggeredState : IEnemyState
{
    public void Enter(EnemyAIController ai)
    {
        // Do animation change
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        ai.context.staggerTimer -= dt;
        if (ai.context.staggerTimer <= 0f)
            ai.ChangeState(ai.context.target != null ? EnemyStates.MoveIntoRange : EnemyStates.Idle);
    }

    public void Exit(EnemyAIController ai)
    {
        // reset animation trigger
    }

}