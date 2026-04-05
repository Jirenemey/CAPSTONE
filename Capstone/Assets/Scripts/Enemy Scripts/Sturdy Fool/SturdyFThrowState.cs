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

        sturdyFool.ResetAnimationFinished();
        enemy.movement.Stop();
        enemy.FacePlayer();

        sturdyFool.SetThrowTarget(sturdyFool.detection.FindClosestPlayer().position);

        enemy.Anim.SetTrigger(SturdyFoolAI.ProjectileHash);
        AudioManager.instance.PlaySFX("SF Throw");
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
        sturdyFool.SetAttackCooldown();
    }
}
