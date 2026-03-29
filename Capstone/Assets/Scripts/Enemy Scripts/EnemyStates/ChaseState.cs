using UnityEngine;
public class ChaseState : EnemyState
{
    private EnemyAI vengefly;
    private Vector2 dir;
    public ChaseState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        vengefly = enemy as EnemyAI;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("ENTERED CHASE STATE");

        enemy.Anim.SetBool(EnemyAI.IsChasingHash, true);
    }

    public override void Update()
    {
        base.Update();

        enemy.FacePlayer();

        dir = enemy.detection.DirectionToPlayer();
        enemy.movement.Move(dir);

        if (enemy.detection.PlayerExitedSight())
        {
            //sm.ChangeState(enemy.DefaultState);
            ChangeState<IdleState>();
        }
    }

    public override void Exit()
    {
        base.Exit();

        enemy.Anim.SetBool(EnemyAI.IsChasingHash, false);
    }
}
