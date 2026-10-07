using UnityEngine;

public class Player_CounterAttackState : PlayerState
{
    private Player_Combat combat;
    private bool counteredSomebody = false;//防止动画没播完就切回idle

    public Player_CounterAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        combat = player.GetComponent<Player_Combat>();
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = combat.GetCounterRecoveryDuration();
        counteredSomebody = combat.CounterAttackPerformed();

        anim.SetBool("counterAttackPerformed", counteredSomebody);
        
    }

    public override void Update()
    {
        base.Update();
        player.SetVelocity(0, rb.linearVelocityY);

        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);

        if(stateTimer < 0 && !counteredSomebody)
            stateMachine.ChangeState(player.idleState);
    }
}
