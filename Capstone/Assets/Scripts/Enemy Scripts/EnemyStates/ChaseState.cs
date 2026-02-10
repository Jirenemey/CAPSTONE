using UnityEngine;

public class ChaseState : EnemyState
{
    public ChaseState(EnemyAI enemy, EnemyStateMachine sm) : base(enemy, sm){}

    public override void Enter()
    {
        Debug.Log("ENTERED CHASE STATE");
    }

    public override void Update()
    {
        enemy.MoveTowardsPlayer();

        if (!enemy.CanSeePlayer())
        {
            sm.ChangeState(new IdleState(enemy, sm));
        }
    }

    public override void Exit()
    {
        Debug.Log("EXITED CHASE STATE");
    }
}
