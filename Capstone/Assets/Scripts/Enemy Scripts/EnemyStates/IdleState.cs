using UnityEngine;
public class IdleState : EnemyState
{
    public IdleState(EnemyAI enemy, EnemyStateMachine sm) : base(enemy, sm) {}

    public override void Enter()
    {
        base.Enter();
        Debug.Log("ENTERED IDLE STATE");
    }

    public override void Update()
    {
        base.Update();

        if (enemy.detection.PlayerEnteredSight())
        {
            sm.ChangeState(enemy.ChaseState);
        }
    }

    public override void Exit()
    {
        Debug.Log("EXITED IDLE STATE");
    }
}
