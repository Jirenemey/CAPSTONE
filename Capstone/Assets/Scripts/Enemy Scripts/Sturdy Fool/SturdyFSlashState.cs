using UnityEngine;

public class SturdyFSlashState : EnemyState
{
    private SturdyFoolAI sturdyFool;

    public SturdyFSlashState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sturdyFool = enemy as SturdyFoolAI;
    }

    public override void Enter()
    {
        base.Enter();

        sturdyFool.ResetAnimationFinished();
        enemy.movement.Stop();
        enemy.FacePlayer();
        enemy.Anim.SetTrigger(SturdyFoolAI.SlashHash);
        AudioManager.instance.PlaySFX("SF Slash");
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
