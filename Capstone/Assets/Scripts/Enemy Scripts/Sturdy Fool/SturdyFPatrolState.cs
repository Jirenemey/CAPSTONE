using UnityEngine;

public class SturdyFPatrolState : EnemyState
{
    private SturdyFoolAI sturdyFool;
    private float lastFlipTime;
    private float flipCooldown = 0.2f;
    private float playerY;

    public SturdyFPatrolState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sturdyFool = enemy as SturdyFoolAI;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.Anim.SetBool(SturdyFoolAI.IsMovingHash, true);
        enemy.StartLoopSFX("SF Walk");
    }

    public override void Update()
    {
        base.Update();

        bool noGroundAhead = !sturdyFool.IsLedgeAhead();
        bool hitWall = sturdyFool.IsWallAhead();

        if (!enemy.IsGrounded())
        {
            ChangeState<SturdyFIdleState>();
        }

        // Edge / wall check
        if ((noGroundAhead || hitWall) && Time.time > lastFlipTime + flipCooldown)
        {
            enemy.Flip();
            lastFlipTime = Time.time;
        }

        playerY = enemy.detection.PlayerRelativeHeight();
        // Player detection
        if (enemy.detection.PlayerEnteredSight() && Time.time >= sturdyFool.NextAttackTime)
        {
            float distance = enemy.detection.DistanceToPlayer();
            float rand = Random.value;

            if (distance <= sturdyFool.MeleeRange && (Mathf.Abs(playerY) <= sturdyFool.MeleeHeight))
            {
                if (rand < 0.7f)
                    ChangeState<SturdyFSlashState>();
                else
                    ChangeState<SturdyFEvadeState>();
            }
            else if (distance <= sturdyFool.RangedRange)
            {
                if (rand < 0.8f)
                    ChangeState<SturdyFThrowState>();
                else
                    ChangeState<SturdyFEvadeState>();
            }
        }

        // Small random idle
        if (Random.value < 0.001f)
        {
            ChangeState<SturdyFIdleState>();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        enemy.movement.Move(Vector2.right * enemy.FacingDirection);
    }

    public override void Exit()
    {
        base.Exit();

        enemy.Anim.SetBool(SturdyFoolAI.IsMovingHash, false);
        enemy.StopLoopSFX();
    }
}
