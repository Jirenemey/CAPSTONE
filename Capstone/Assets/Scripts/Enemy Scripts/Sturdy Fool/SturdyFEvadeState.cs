using UnityEngine;

public class SturdyFEvadeState : EnemyState
{
    private SturdyFoolAI sturdyFool;
    private float evadeDir;

    public SturdyFEvadeState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sturdyFool = enemy as SturdyFoolAI;
    }

    public override void Enter()
    {
        base.Enter();

        sturdyFool.ResetAnimationFinished();
        enemy.movement.Stop();
        enemy.FacePlayer();

        evadeDir = -enemy.detection.PlayerLeftOrRight();

        enemy.Anim.SetTrigger(SturdyFoolAI.EvadeHash);
        AudioManager.instance.PlaySFX("SF Evade");
    }

    public override void Update()
    {
        base.Update();

        if (!sturdyFool.IsGrounded())
        {
            ChangeState<SturdyFIdleState>();
        }

        if (sturdyFool.AnimationFinished)
        {
            enemy.movement.Stop();
            ChangeState<SturdyFThrowState>();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (sturdyFool.Applyforce)
            enemy.RB.linearVelocity = new Vector2(evadeDir * sturdyFool.EvadeSpeed, 0f);
        else
            enemy.movement.Stop();
    }

    public override void Exit()
    {
        base.Exit();
        sturdyFool.ResetAnimationFinished();
    }
}
