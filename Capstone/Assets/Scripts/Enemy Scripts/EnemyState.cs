using UnityEngine;

public abstract class EnemyState
{
    protected EnemyBase enemy;
    protected EnemyStateMachine sm;

    protected float stateTimer;

    public EnemyState(EnemyBase enemy, EnemyStateMachine sm)
    {
        this.enemy = enemy;
        this.sm = sm;
    }

    public virtual void Enter() { stateTimer = 0f; }
    public virtual void Update() { stateTimer += Time.deltaTime; }

    public virtual void FixedUpdate() { }

    public virtual void Exit() { }

    protected void ChangeState<T>() where T : EnemyState
    {
        var newState = enemy.GetState<T>();
        if (newState != null)
            sm.ChangeState(newState);
    }
}
