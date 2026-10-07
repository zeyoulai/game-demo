using UnityEngine;

public class Enemy_BattleState : EnemyState
{

    private Transform player;
    private Transform lastTarget;
    private float lastTimeWasBattle;
    protected float lastTimeAttacked = float.NegativeInfinity;
    
    public Enemy_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        //可能是没检测到玩家，而直接进入战斗状态
        UpdateBattleTimer();

        //player ??= enemy.GetPlayerReference(); //与下面的写法等价
        if(player == null)
            player = enemy.GetPlayerReference();


        if (ShouldRetreat())
        {
            ShortRetreat();
        }

    }

    protected void ShortRetreat()
    {
        float x = (enemy.retreatVelocity.x * enemy.activeSlowMultiplier) *-DirectionToPlayer();
        float y = enemy.retreatVelocity.y;
        rb.linearVelocity = new Vector2(x, y); //不用setVelocity，因为这样会自动Flip
        enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        base.Update();

        if (enemy.PlayerDetected() == true)
        {
            UpdateTargetIfNeeded();
            UpdateBattleTimer();

        }
        if(BattleTimeIsOver())
            stateMachine.ChangeState(enemy.idleState);

        if (WithinAttackRange() && enemy.PlayerDetected() && CanAttack())
        {

            lastTimeAttacked = Time.time;
            stateMachine.ChangeState(enemy.attackState);
        }
        else
        {
            float xVelocity = enemy.canChasePlayer ? enemy.GetBattleMoveSpeed() : 0.0001f;
            enemy.SetVelocity(xVelocity * DirectionToPlayer(), rb.linearVelocity.y);
        }


    }

    protected virtual bool CanAttack() => Time.time > lastTimeAttacked + enemy.attackCooldown;

    protected void UpdateTargetIfNeeded()
    {
        if (enemy.PlayerDetected() == false)
            return;

        Transform newTarget = enemy.PlayerDetected().transform;
        if(newTarget != lastTarget)
        {
            lastTarget = newTarget;
            player = newTarget;
        }
    }


    protected void UpdateBattleTimer() => lastTimeWasBattle = Time.time;

    protected bool BattleTimeIsOver() => Time.time > lastTimeWasBattle + enemy.battleTimeDuration;

    protected bool WithinAttackRange() => DistanceToPlayer() < enemy.attackDistance;

    protected bool ShouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;


    protected float DistanceToPlayer()
    {
        if(player == null)
            return float.MaxValue;
        return Mathf.Abs(player.position.x - enemy.transform.position.x);
    }

    protected int DirectionToPlayer()
    {
        if(player == null)
            return 0;
        return player.position.x >enemy.transform.position.x ? 1 : -1;
    }
}
