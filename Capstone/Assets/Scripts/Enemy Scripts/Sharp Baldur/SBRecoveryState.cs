using UnityEngine;

public class SBRecoveryState : EnemyState
{
    private SharpBaldurAI sharpBaldur;
    private bool cooldownStarted = false    ;

    public SBRecoveryState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sharpBaldur = enemy as SharpBaldurAI;
    }

    public override void Enter()
    {
        base.Enter();

        cooldownStarted = false;
        enemy.movement.Stop();
        sharpBaldur.ResetAnimationFinished();
    }

    public override void Update()
    {
        base.Update();

        if (!sharpBaldur.AnimationFinished)
            return;

        if (!cooldownStarted)
        {
            stateTimer = 0f;
            cooldownStarted = true;
        }

        if (stateTimer >= sharpBaldur.AttackCooldown)
            ChangeState<SBIdleState>();
    }

    public override void Exit()
    {
        base.Exit();
        sharpBaldur.ResetAnimationFinished();
        enemy.FacePlayer();
    }
}
