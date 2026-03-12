using UnityEngine;

public abstract class EnemyState
{
    protected EnemyAI enemy;
    protected EnemyStateMachine sm;

    protected float stateTimer;

    public EnemyState(EnemyAI enemy, EnemyStateMachine sm)
    {
        this.enemy = enemy;
        this.sm = sm;
    }

    public virtual void Enter() { stateTimer = 0f; }
    public virtual void Update() { stateTimer += Time.deltaTime; }
    public virtual void Exit() { }
}
