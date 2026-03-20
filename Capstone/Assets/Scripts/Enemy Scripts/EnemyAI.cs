using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public class EnemyAI : EnemyBase
{
    [Header("Enemy Specific")]
    public float attackRadius = 2;
    public float windupTime = 0.5f;
    public float recoveryTime = 0.5f;
    public float attackCooldown = 2.0f;

    private float lastAttackTime;

    //states
    public IdleState IdleState { get; private set; }
    public ChaseState ChaseState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        IdleState = new IdleState(this, fsm);
        ChaseState = new ChaseState(this, fsm);
        DefaultState = IdleState;

        fsm.ChangeState(IdleState);
    }
    void Start()
    {

    }

    protected override void Update()
    {
        base.Update();
    }

    public bool CanAttackPlayer()
    {
        if (attackCooldown <= lastAttackTime)
        {
            lastAttackTime = 0;
            return true;
        }
        lastAttackTime += Time.deltaTime;
        return false;
    }
}
