using UnityEngine;

public class SturdyFIdleState : EnemyState
{
    private SturdyFoolAI sturdyFool;

    public SturdyFIdleState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sturdyFool = enemy as SturdyFoolAI;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.movement.Stop();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.IsGrounded() && stateTimer >= 1f)
        {
            ChangeState<SturdyFPatrolState>();
        }
    }
}
