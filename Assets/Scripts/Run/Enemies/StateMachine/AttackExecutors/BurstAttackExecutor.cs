using System.Collections;
using UnityEngine;

public class BurstAttackExecutor : ProjectileAttackExecutor
{
    [SerializeField] private int burstCount;
    [SerializeField] private float burstInterval;

    private Coroutine burstRoutine;

    public override void Execute(EnemyAIController ai)
    {
        if (hasFired) return;
        
        if (laserSight != null && laserSight.enabled)
            laserSight.enabled = false;
            
        Executing = true;

        if (burstRoutine != null)
        {
            StopCoroutine(burstRoutine);
            burstRoutine = null;
        }

        burstRoutine = ai.StartCoroutine(BurstFireRoutine(ai));

        hasFired = true;
    }

    private IEnumerator BurstFireRoutine(EnemyAIController ai)
    {
        for (int i = 0; i < burstCount; i++)
        {
            Vector2 dir = (Vector2)ai.context.target.position - ai.Body.position;
            FireSingle(projectiles[0], dir);

            yield return new WaitForSeconds(burstInterval);
        }

        Executing = false;
    }

    public override void Interrupt(EnemyAIController ai)
    {
        base.Interrupt(ai);

        if (burstRoutine != null)
        {
            StopCoroutine(burstRoutine);
            burstRoutine = null;
        }
    }

}
