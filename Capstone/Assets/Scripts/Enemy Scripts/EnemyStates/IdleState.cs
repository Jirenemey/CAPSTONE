using UnityEngine;
public class IdleState : EnemyState
{
    public IdleState(EnemyAI enemy, EnemyStateMachine sm) : base(enemy, sm) {}

    public override void Enter()
    {
        Debug.Log("ENTERED IDLE STATE");
    }

    public override void Update()
    {
        if (enemy.CanSeePlayer())
        {
            sm.ChangeState(new ChaseState(enemy, sm));
        }
    }

    public override void Exit()
    {
        Debug.Log("EXITED IDLE STATE");
    }
}
