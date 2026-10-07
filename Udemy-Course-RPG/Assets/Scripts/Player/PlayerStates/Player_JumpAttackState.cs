using UnityEngine;

public class Player_JumpAttackState : PlayerState
{
    private bool touchedGround;

    public Player_JumpAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        touchedGround = false;

        player.SetVelocity(player.jumpAttackVelocity.x * player.facingDir, player.jumpAttackVelocity.y); //这里设置了速度.首先执行的是jump的父对象（空中状态）的update,然后是在父状态中检测的是否跳跃攻击（1），然后才在jump状态执行判断是否fall，这个时候已经进入到跳跃攻击的setVelocity了（2），然后另外一边又执行到判断下落（3），于是就下落了，这就是代码执行顺序，debug过了就是这样
        
    }

    public override void Update()
    {
        base.Update();

        if (player.groundDetected && touchedGround == false)
        {
            touchedGround = true;
            anim.SetTrigger("jumpAttackTrigger");
            player.SetVelocity(0,rb.linearVelocity.y);
        }

        if (triggerCalled && player.groundDetected)
            stateMachine.ChangeState(player.idleState);
    }

    public override void Exit()
    {
        base.Exit();

    }



}
