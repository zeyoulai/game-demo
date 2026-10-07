using UnityEngine;

public class Player_DashState : PlayerState
{

    private float orginalGravityScale;
    private int dashDir;



    public Player_DashState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        skillManager.dash.OnStartEffect();
        player.vfx.DoImageEchoEffect(player.dashDuration);

        stateTimer = player.dashDuration;

        orginalGravityScale = rb.gravityScale;
        rb.gravityScale = 0;
        dashDir = player.moveInput.x != 0 ? ((int)player.moveInput.x) : player.facingDir;

        player.health.SetCanTakenDamage(false);
        player.gameObject.layer = LayerMask.NameToLayer("Untargetable");

    }

    public override void Update()
    {
        base.Update();
        cancelDashIfNeeded();
        player.SetVelocity(player.dashSpeed* dashDir, 0);   //保证冲刺过程不会改变方向,保护措施，一般情况不需要，但是作者说以前遇到过这种情况

        if(stateTimer < 0)
        {
            if (player.groundDetected)
                stateMachine.ChangeState(player.idleState);
            else
                stateMachine.ChangeState(player.fallState);
        }

    }

    public override void Exit()
    {
        base.Exit();
        skillManager.dash.OnEndEffect();
        player.SetVelocity(0, 0);
        rb.gravityScale = orginalGravityScale;
        player.dashCoolDown = player.dashLimitDuration;
        player.health.SetCanTakenDamage(true);
        player.gameObject.layer = LayerMask.NameToLayer("Player");

    }

    private void cancelDashIfNeeded()
    {
        if (player.wallDetected)
        {
            if (player.groundDetected)
            {
                stateMachine.ChangeState(player.idleState);
            }
            else
            {
                stateMachine.ChangeState(player.wallSlideState);
            }
        }
    }


}
