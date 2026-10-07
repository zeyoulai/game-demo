using UnityEngine;

public class Player_BasicAttackState : PlayerState
{

    private float attackVelocityTimer;
    private float lastTimeAttacked;

    private bool comboAttackQueued;
    private int attackDir;
    private int comboIndex = 1;
    private int comboLimit = 3;
    private const int FirstComboIndex = 1;  //we start combo index with number 1 this paramter is used in the Animation

    public Player_BasicAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        if(comboLimit != player.attackVelocity.Length)
            comboLimit = player.attackVelocity.Length;
    }

    public override void Enter()
    {
        base.Enter();
        comboAttackQueued = false;
        ResetComboIndexIfNeeded();
        SyncAttackSpeed();

        attackDir = player.moveInput.x != 0 ? ((int)player.moveInput.x) : player.facingDir;

        anim.SetInteger("basicAttackIndex", comboIndex);
        ApplyAttackVelocity();
    }



    public override void Update()
    {
        base.Update();
        HandleAttackVelocity();
        if (input.Player.Attack.WasPressedThisFrame())
            QueueNextAttack();

        if (triggerCalled)
            HandleStateExit();

        //攻击时也能反击
        //if (input.Player.CounterAttack.WasPressedThisFrame())
        //    stateMachine.ChangeState(player.counterAttackState);


        //第一段攻击动作有点慢了，如何中断这个动作直接进入下一个状态呢？ Todo 其实下一个视频就是解决的这个问题   这个问题其实是这样的，一帧里面会执行多行代码，所以很多时候不能保证进入下一帧的时候你的变量状态是什么样子的，这就可能会导致我们根本就不会改变bool值，这一帧是true，下一帧也是true  原因很可能是一帧内点用携程。
        //if (input.Player.Attack.WasPressedThisFrame())
        //{
        //    comboIndex++;
        //    stateMachine.ChangeState(player.basicAttackState);
        //}
    }

    private void HandleStateExit()
    {
        if (comboAttackQueued)
        {
            anim.SetBool(animBoolName, false);//取消剩下的动作.在这一帧内。因为下面的携程函数写成了下一帧才会执行changestate  所以基本上会在这一帧退出现在的状态，下一帧进入basicattack的状态
            player.EnterAttaclStateWithDelay();
        }
        else
            stateMachine.ChangeState(player.idleState);
    }

    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;
        if (attackVelocityTimer < 0)
            player.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();

        comboIndex++;
        //remember time when we attacked 
        lastTimeAttacked = Time.time;
    }
    private void QueueNextAttack()
    {
        if (comboIndex < comboLimit)
            comboAttackQueued = true;
    }

    private void ApplyAttackVelocity()
    {
        Vector2 attackVelocity = player.attackVelocity[comboIndex - FirstComboIndex];
        player.SetVelocity(attackVelocity.x * attackDir, attackVelocity.y);
        attackVelocityTimer = player.attackVelocityDuration;
    }

    private void ResetComboIndexIfNeeded()
    {
        // if time is long ago ,reset combo index
        if(Time.time>player.comboResetTime+lastTimeAttacked)
            comboIndex = FirstComboIndex;

        if (comboIndex > comboLimit)
            comboIndex = FirstComboIndex;

    }
}
