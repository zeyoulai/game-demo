public class Player_WallJumpState : PlayerState
{
    public Player_WallJumpState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.SetVelocity(player.wallJumpForce.x * -player.facingDir, player.wallJumpForce.y);
    }


    public override void Update()
    {
        base.Update();

        if (rb.linearVelocity.y < 0)
            stateMachine.ChangeState(player.fallState);

        if (player.wallDetected)
            stateMachine.ChangeState(player.wallSlideState);

        //下面这段自己加的，为的是在walljump的前半段状态下也能够左右移动
        if (player.wallJumpINF)
        {
            if (player.moveInput.x != 0)
            {
                player.SetVelocity(player.moveInput.x * player.moveSpeed * player.inAirMoveMultiplier, rb.linearVelocity.y);
            }
            if (input.Player.Attack.WasPressedThisFrame())
            {
                stateMachine.ChangeState(player.jumpAttackState);
            }
        }

    }
}
