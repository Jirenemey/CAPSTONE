using UnityEngine;

public class SturdyFThrowState : EnemyState
{
    private SturdyFoolAI sturdyFool;

    public SturdyFThrowState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sturdyFool = enemy as SturdyFoolAI;
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log("ENTERED SF THROW STATE");

        sturdyFool.ResetAnimationFinished();
        enemy.movement.Stop();
        enemy.FacePlayer();
        enemy.Anim.SetTrigger(SturdyFoolAI.ProjectileHash);
    }

    public override void Update()
    {
        base.Update();

        if (sturdyFool.AnimationFinished)
        {
            ChangeState<SturdyFIdleState>();
        }
    }

    public override void Exit()
    {
        base.Exit();
        sturdyFool.ResetAnimationFinished();
    }
}
