using UnityEngine;

public class AttackState : EnemyState
{
    public AttackState(EnemyAI enemy, EnemyStateMachine sm) : base(enemy, sm){}

    public override void Enter()
    {
        base.Enter();   
        Debug.Log("ENTERED ATTACK STATE");
    }

    public override void Update()
    {
        base.Update();

        if (enemy.detection.InAttackRange() && enemy.CanAttackPlayer())
        {
            Debug.Log("Attacked");
        }
        else
        {
            sm.ChangeState(enemy.IdleState);
        }
    }

    public override void Exit()
    {
        Debug.Log("EXITED ATTACK STATE");
    }
}
