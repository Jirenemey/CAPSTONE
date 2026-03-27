public class EnemyStateMachine
{
    public EnemyState CurrentState { get; private set; }

    public void ChangeState(EnemyState newState)
    {
        if (newState == null) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void UpdateState()
    {
        CurrentState?.Update();
    }

    public void FixedUpdateState()
    {
        CurrentState?.FixedUpdate();
    }
}
