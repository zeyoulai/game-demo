using UnityEngine;

public class Player_FallState : Player_AirState
{
    public Player_FallState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
 
    }

    public override void Update()
    {
        base.Update();

        // if player detecting ground ,go to idle
        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
        }

        if (player.wallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
        }


    }
    public override void Exit()
    {
        base.Exit();

    }
}
