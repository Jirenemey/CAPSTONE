using UnityEngine;

public class SturdyFEvadeState : EnemyState
{
    private SturdyFoolAI sturdyFool;

    public SturdyFEvadeState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sturdyFool = enemy as SturdyFoolAI;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("ENTERED SF EVADE STATE");

        sturdyFool.ResetAnimationFinished();
        enemy.movement.Stop();
        enemy.FacePlayer();
        enemy.Anim.SetTrigger(SturdyFoolAI.EvadeHash);
    }

    public override void Update()
    {
        base.Update();

        if (sturdyFool.AnimationFinished)
        {
            ChangeState<SturdyFThrowState>();
        }
    }

    public override void Exit()
    {
        base.Exit();
        sturdyFool.ResetAnimationFinished();
    }
}
