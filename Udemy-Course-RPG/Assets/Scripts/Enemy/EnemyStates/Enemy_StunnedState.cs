using UnityEngine;

public class Enemy_StunnedState : EnemyState
{
    private Enemy_VFX vfx;

    public Enemy_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        vfx = enemy.GetComponent<Enemy_VFX>();
    }

    public override void Enter()
    {
        base.Enter();

        enemy.EnableCounterWindoow(false);// 如果手动反击，disable动画事件会被打断，所以手动设置一下
        vfx.EnableAttackAlert(false);

        stateTimer = enemy.stunnedDutation;
        rb.linearVelocity = new Vector2(enemy.stunnedVelocity.x * -enemy.facingDir, enemy.stunnedVelocity.y);

    }

    public override void Update()
    {
        base.Update();

        if(stateTimer < 0)
        {
            //Debug.Log("进入 idle状态");
            //enemy.EnableCounterWindoow(false);// 如果手动反击，disable动画事件会被打断，所以手动设置一下
            //vfx.EnableAttackAlert(false);
            stateMachine.ChangeState(enemy.idleState);
        }
    }


}
