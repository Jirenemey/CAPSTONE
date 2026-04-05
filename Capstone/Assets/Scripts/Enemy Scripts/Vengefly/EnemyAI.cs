using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.PlayerLoop.PreUpdate;

public class EnemyAI : EnemyBase
{
    //[Header("Enemy Specific")]

    public static readonly int IsChasingHash = Animator.StringToHash("isChasing");

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        StartLoopSFX("Vengefly Fly Loop");
    }

    protected override void RegisterStates()
    {
        AddState(new IdleState(this, fsm));
        AddState(new ChaseState(this, fsm));
        AddState(new AttackState(this, fsm));
        
        fsm.ChangeState(GetState<IdleState>());
    }

    protected override void Die()
    {
        StopLoopSFX();

        base.Die();
    }
}
