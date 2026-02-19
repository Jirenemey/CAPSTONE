using UnityEngine;

public class AttackState : EnemyState
{
    public AttackState(EnemyAI enemy, EnemyStateMachine sm) : base(enemy, sm){}

    public override void Enter()
    {
        Debug.Log("ENTERED ATTACK STATE");
    }

    public override void Update()
    {
        if (enemy.InAttackRange() && enemy.CanAttackPlayer())
        {
            Debug.Log("Attacked");
        }
        else
        {
            sm.ChangeState(new IdleState(enemy, sm));
        }
    }

    public override void Exit()
    {
        Debug.Log("EXITED ATTACK STATE");
    }
}
