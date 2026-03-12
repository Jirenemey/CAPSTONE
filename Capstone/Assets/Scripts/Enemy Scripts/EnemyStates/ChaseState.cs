using UnityEngine;
public class ChaseState : EnemyState
{
    public ChaseState(EnemyAI enemy, EnemyStateMachine sm) : base(enemy, sm){}

    public override void Enter()
    {
        base.Enter();
        Debug.Log("ENTERED CHASE STATE");
    }

    public override void Update()
    {
        base.Update();

        enemy.MoveTowardsPlayer();

        if (!enemy.CanSeePlayer())
        {
            sm.ChangeState(enemy.DefaultState);
        }
    }

    public override void Exit()
    {
        Debug.Log("EXITED CHASE STATE");
    }
}
