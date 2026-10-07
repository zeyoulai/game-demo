using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家普通攻击状态
/// </summary>
public class PlayerNormalAttackState : PlayerStateBase
{
    private bool enterNextAttack;

    public override void Enter()
    {
        base.Enter();

        enterNextAttack = false;
        //播放攻击动画
        playerController.PlayAnimation("Attack_Normal_" + playerModel.skiilConfig.currentNormalAttackIndex, 0.1f) ;
    }

    public override void Update()
    {
        base.Update();

        #region 检测大招
        if (playerController.inputSystem.Player.BigSkill.triggered)
        {
            //切换到进入大招状态
            playerController.SwitchState(PlayerState.BigSkillStart);
            return;
        }
        #endregion

        // 判断是否连击,缓冲
        if (NormalizedTime() >= 0.5f && playerController.inputSystem.Player.Fire.triggered) 
        {
            enterNextAttack = true;
        }

        #region 动画是否播放结束
        if (IsAnimationEnd())
        {
            if (enterNextAttack)
            {
                // 切换到下一个普攻
                //攻击段数累加
                playerModel.skiilConfig.currentNormalAttackIndex++;
                if (playerModel.skiilConfig.currentNormalAttackIndex > playerModel.skiilConfig.normalAttackDamageMultiple.Length)
                {
                    // 当前动机段数归零
                    playerModel.skiilConfig.currentNormalAttackIndex = 1;
                }

                //切换到普通攻击状态
                playerController.SwitchState(PlayerState.NormalAttack);
                return;
            }
            else
            {
                //切换到普通攻击后摇状态
                playerController.SwitchState(PlayerState.NormalAttackEnd);
                return;
            }
        }
        #endregion
    }
}
