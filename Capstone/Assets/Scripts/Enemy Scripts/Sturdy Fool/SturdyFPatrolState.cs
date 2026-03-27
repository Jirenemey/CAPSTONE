using UnityEngine;

public class SturdyFPatrolState : EnemyState
{
    private SturdyFoolAI sturdyFool;

    public SturdyFPatrolState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sturdyFool = enemy as SturdyFoolAI;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("ENTERED SF PATROL STATE");
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
