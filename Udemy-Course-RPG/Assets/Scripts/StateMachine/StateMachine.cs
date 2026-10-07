public class StateMachine
{
    public EntityState currentSate
    {
        get; private set;
    }
    public bool canChangeState = true;

    public void Initialize(EntityState startState)
    {
        canChangeState = true;
        currentSate = startState;
        currentSate.Enter();
    }

    public void ChangeState(EntityState newState)
    {
        if (canChangeState == false)
            return;

        currentSate.Exit();
        currentSate = newState;
        currentSate.Enter();
    }

    public void UpdateActiveState()
    {
        currentSate.Update();
    }

    public void SwitchOffStateMachine() => canChangeState = false;
}
