using UnityEngine;
public class IdleState : EnemyState
{
    public IdleState(EnemyBase enemy, EnemyStateMachine sm) : base(enemy, sm) {}

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
            //AudioManager.instance.PlaySFX("Vengefly Startle");
            ChangeState<ChaseState>();
        }
    }
}
