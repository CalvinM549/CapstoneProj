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
            ai.ChangeState(ai.PostAggroState);
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
        var executor = ai.GetPrimaryExecutor();

        if (executor != null)
        {
            ctx.currentAttackRange = ai.GetPrimaryExecutor().AttackData.attackRange;
        }
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        var ctx = ai.context;

        if (ctx.target == null)
        {
            ai.ChangeState(EnemyStates.Idle);
            return;
        }

        if (ai.GetPunishExecutor() != null)
        {
            ai.ChangeState(EnemyStates.Attack);
            return;
        }

        if (ctx.distanceToTarget < ctx.currentAttackRange && AIManager.Instance.TryFindTarget(ai, out var target))
        {
            ai.ChangeState(EnemyStates.Attack);
            return;
        }

        Vector2 dir = ai.GetChaseDirection();
        ai.Body.linearVelocity = dir * ai.speedStat.Value;
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
            ai.ChangeState(ai.PostAttackState);
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

        if (ctx.timeSinceAttack >= executor.AttackData.winddownDuration + executor.AttackData.telegraphDuration && !executor.Executing)
        {
            ai.ChangeState(ai.PostAttackState);
            return;
        }

        if (ctx.timeSinceAttack >= executor.AttackData.telegraphDuration && !executor.Executing && !executor.HasExecuted)
        {
            executor.Execute(ai);
            return;
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
    private const float wallLookahead = 0.75f;

    public void Enter(EnemyAIController ai)
    {
        ai.context.stateTimer = ai.profile.repositionDuration > 0
            ? ai.profile.repositionDuration
            : defaultRepositionDuration;

        ai.context.repositionSign = Random.value < 0.5f ? -1 : 1;
    }

    public void Tick(EnemyAIController ai, float dt)
    {
        var ctx = ai.context;

        if (ctx.target == null)
        {
            ai.ChangeState(EnemyStates.Idle);
            return;
        }

        if (ai.GetPunishExecutor() != null)
        {
            ai.ChangeState(EnemyStates.Attack);
            return;
        }

        ctx.stateTimer -= dt;

        if (ai.speedStat.Value > 0f)
        {
            Vector2 pos = ai.Body.position;
            Vector2 toTarget = ((Vector2)ctx.target.position - pos).normalized;
            Vector2 dir = RepositionDirection(ai, toTarget, ctx.repositionSign);

            INavigation nav = AIManager.Instance.Nav;

            if (nav != null && !nav.HasClearPath(pos, pos + dir * wallLookahead, ai.profile.agentRadius, ai.profile.obstacleLayer))
            {
                ctx.repositionSign = -ctx.repositionSign;

                dir = RepositionDirection(ai, toTarget, ctx.repositionSign);

                if (!nav.HasClearPath(pos, pos + dir * wallLookahead, ai.profile.agentRadius, ai.profile.obstacleLayer))
                    dir = Vector2.zero;
            }

            float speed = ai.speedStat.Value * (ai.profile.repositionSpeedMult > 0f ? ai.profile.repositionSpeedMult : 1f);
            ai.Body.linearVelocity = dir * speed;
        }

        if (ctx.stateTimer <= 0f)
            ai.ChangeState(EnemyStates.MoveIntoRange);
    }

    public void Exit(EnemyAIController ai)
    {
        ai.Body.linearVelocity = Vector2.zero;
    }

    private static Vector2 RepositionDirection(EnemyAIController ai, Vector2 toTargetDir, int sign)
    {
        Vector2 lateral = Vector2.Perpendicular(toTargetDir) * sign;

        return ai.profile.repositionAwayFromTarget
            ? (lateral - toTargetDir).normalized
            : lateral;
    }
}

public class StaggeredState : IEnemyState
{
    public void Enter(EnemyAIController ai)
    {
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