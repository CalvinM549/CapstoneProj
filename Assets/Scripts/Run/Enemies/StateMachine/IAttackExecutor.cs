using UnityEngine;

public interface IAttackExecutor
{
    EnemyAttackData AttackData { get; }
    bool Executing { get; }
    bool CanExecute(EnemyAIController ai); // Attack checks, i.e. range/los/ammo
    void BeginTelegraph(EnemyAIController ai); // windup animation
    void Execute(EnemyAIController ai); // firing projecitles / do melee / wind down
    void Interrupt(EnemyAIController ai); // Interrupt
}
