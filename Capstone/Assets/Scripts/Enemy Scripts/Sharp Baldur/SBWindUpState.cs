using UnityEngine;

public class SBWindUpState : EnemyState
{
    private SharpBaldurAI sharpBaldur;

    public SBWindUpState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sharpBaldur = enemy as SharpBaldurAI;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.FacePlayer();
        sharpBaldur.ResetAnimationFinished();
        enemy.Anim.SetTrigger(SharpBaldurAI.StartRollHash);
        AudioManager.instance.PlaySFX("SB Curl");
    }

    public override void Update()
    {
        base.Update();

        if (sharpBaldur.AnimationFinished)
        {
            ChangeState<SBRollState>();
        }
    }

    public override void Exit()
    {
        base.Exit();
        sharpBaldur.ResetAnimationFinished();
    }
}
