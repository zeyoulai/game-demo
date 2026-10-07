using UnityEngine;

public class Enemy_ArcherElfBattleState : Enemy_BattleState
{
    public bool canFlip;
    public bool reachedDeadEnd;

    public Enemy_ArcherElfBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        reachedDeadEnd = false;
    }

    public override void Update()
    {
        // we going to run logic of the state here
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        if (enemy.groundDetected == false || enemy.wallDetected)
        {
            reachedDeadEnd = true;
        }

        if (enemy.PlayerDetected() == true)
        {
            UpdateTargetIfNeeded();
            UpdateBattleTimer();

        }
        if (BattleTimeIsOver())
            stateMachine.ChangeState(enemy.idleState);

        if (CanAttack())
        {
            if(enemy.PlayerDetected() == false && canFlip)
            {
                enemy.HandleFlip(DirectionToPlayer());
                canFlip = false;
            }

            enemy.SetVelocity(0,rb.linearVelocityY);

            if (WithinAttackRange() && enemy.PlayerDetected() && CanAttack())
            {
                canFlip = true;
                lastTimeAttacked = Time.time;
                stateMachine.ChangeState(enemy.attackState);
            }
        }
        else
        {
            //float xVelocity = enemy.canChasePlayer ? enemy.GetBattleMoveSpeed() : enemy.GetBattleMoveSpeed() * -1;
            bool shouldWalkAway = reachedDeadEnd == false && DistanceToPlayer() < (enemy.attackDistance * .65f);

            if (shouldWalkAway)
            {
                enemy.SetVelocity((enemy.GetBattleMoveSpeed() * -1) * DirectionToPlayer(), rb.linearVelocity.y);
            }
            else
            {
                enemy.SetVelocity(0, rb.linearVelocity.y);

                if (enemy.PlayerDetected() == false)
                    enemy.HandleFlip(DirectionToPlayer());
            }
        }


    }
}
