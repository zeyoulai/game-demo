using UnityEngine;

public class Player_JumpState : Player_AirState
{
    public Player_JumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        //make object go up ,increase y velocity
        player.SetVelocity(rb.linearVelocity.x, player.jumpForce);

    }

    public override void Update()
    {

        base.Update();
        if (rb.linearVelocity.y < 0 && stateMachine.currentSate != player.jumpAttackState)
        {

            stateMachine.ChangeState(player.fallState);
        }

    }

    public override void Exit()
    {
        base.Exit();


        // if y velocity goes down charcater is falling transfer to fillstate
    }

}
