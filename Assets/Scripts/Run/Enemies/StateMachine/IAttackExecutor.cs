using UnityEngine;

public interface IAttackExecutor
{
    float TelegraphDuration { get; } // Windup Duration
    bool CanExecute(EnemyAIController ai); // Attack checks, i.e. range/los/ammo
    void BeginTelegraph(EnemyAIController ai); // windup animation
    void Execute(EnemyAIController ai); // firing projecitles / do melee / wind down
    void CancelTelegraph(EnemyAIController ai); // Interrupt
}
