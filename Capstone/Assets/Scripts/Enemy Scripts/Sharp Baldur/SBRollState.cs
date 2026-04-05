using UnityEngine;

public class SBRollState : EnemyState
{
    private SharpBaldurAI sharpBaldur;
    private Vector2 rollDir;

    public SBRollState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sharpBaldur = enemy as SharpBaldurAI;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.Anim.SetBool(SharpBaldurAI.RollHash, true);

        enemy.StartLoopSFX("SB Rolling");

        rollDir = enemy.detection.DirectionToPlayer().normalized;

        rollDir.y = 0;
        rollDir.Normalize();
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer >= sharpBaldur.RollDuration)
        {
            ChangeState<SBRecoveryState>();
            return;
        }

        if (sharpBaldur.HitWall)
        {
            rollDir.x *= -1f;
            enemy.FaceDirection(rollDir.x);
            AudioManager.instance.PlaySFX("SB Hit Wall");
            enemy.RB.linearVelocity = new Vector2(rollDir.x * sharpBaldur.RollSpeed, sharpBaldur.BounceForce);
            sharpBaldur.ResetWallHit();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        Vector2 currentVel = enemy.RB.linearVelocity;
        enemy.RB.linearVelocity = new Vector2(rollDir.x * sharpBaldur.RollSpeed, currentVel.y);
    }

    public override void Exit()
    {
        enemy.Anim.SetBool(SharpBaldurAI.RollHash, false);
        enemy.StopLoopSFX();
        enemy.RB.linearVelocity = new Vector2(0f, enemy.RB.linearVelocity.y);
    }
}
