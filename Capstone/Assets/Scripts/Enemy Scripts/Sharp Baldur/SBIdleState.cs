using UnityEngine;

public class SBIdleState : EnemyState
{
    private SharpBaldurAI sharpBaldur;

    public SBIdleState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm)
    {
        sharpBaldur = enemy as SharpBaldurAI;
    }

    public override void Enter()
    {
        base.Enter();

        enemy.movement.Stop();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.detection.PlayerEnteredSight())
        {
            ChangeState<SBWindUpState>();
        }
    }
}
