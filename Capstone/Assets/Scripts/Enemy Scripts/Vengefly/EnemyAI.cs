using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.PlayerLoop.PreUpdate;

public class EnemyAI : EnemyBase
{
    [Header("Enemy Specific")]
    public float attackRadius = 2;
    public float windupTime = 0.5f;
    public float recoveryTime = 0.5f;
    public float attackCooldown = 2.0f;

    private float lastAttackTime;

    public static readonly int DiedHash = Animator.StringToHash("Died");
    public static readonly int IsChasingHash = Animator.StringToHash("isChasing");

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
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
        base.Die();


    }
}
