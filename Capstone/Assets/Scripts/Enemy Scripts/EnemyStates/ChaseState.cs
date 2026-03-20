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

        //enemy.MoveTowardsPlayer();
        //enemy.Move();
        Vector2 dir = enemy.detection.DirectionToPlayer();
        enemy.movement.Move(dir);

        if (enemy.detection.PlayerExitedSight())
        {
            sm.ChangeState(enemy.DefaultState);
        }
    }

    public override void Exit()
    {
        Debug.Log("EXITED CHASE STATE");
    }
}
