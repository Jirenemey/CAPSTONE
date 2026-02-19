using UnityEngine;

public abstract class EnemyState
{
    protected EnemyAI enemy;
    protected EnemyStateMachine sm;

    public EnemyState(EnemyAI enemy, EnemyStateMachine sm)
    {
        this.enemy = enemy;
        this.sm = sm;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
