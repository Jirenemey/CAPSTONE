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
        Debug.Log("ENTERED SF IDLE STATE");

        enemy.movement.Stop();
    }

    public override void Update()
    {
        base.Update();

        //if (enemy.detection.PlayerEnteredSight())
        //{
        //    ChangeState<SBWindUpState>();
        //}
    }
}
