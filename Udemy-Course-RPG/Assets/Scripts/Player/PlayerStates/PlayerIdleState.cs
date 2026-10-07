using UnityEngine;

public class PlayerIdleState : Player_GroundedState
{
    public PlayerIdleState(Player player, StateMachine stateMachine, string stateName) : base(player,stateMachine, stateName)
    {

    }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(0,rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (player.moveInput.x == player.facingDir && player.wallDetected)
            return;

        if (player.moveInput.x != 0)
        {
            stateMachine.ChangeState(player.moveState);
        }


    }

}
