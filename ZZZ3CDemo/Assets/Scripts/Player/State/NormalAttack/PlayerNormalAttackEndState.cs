using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家普通攻击后摇状态
/// </summary>
public class PlayerNormalAttackEndState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();

        //播放攻击后摇动画
        playerController.PlayAnimation($"Attack_Normal_{ playerModel.skiilConfig.currentNormalAttackIndex}_End", 0.1f) ;
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

        #region 检测攻击

        if (playerController.inputSystem.Player.Fire.triggered)
        {
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
        #endregion

        #region 检测移动
        if (playerController.inputMoveVec2 != Vector2.zero && statePlayTime > 0.5f)
        {
            //切换到移动状态
            playerController.SwitchState(PlayerState.Walk);
            // 当前动机段数归零
            playerModel.skiilConfig.currentNormalAttackIndex = 1;
            return;
        }
        #endregion

        #region 检测闪避
        if (playerController.inputSystem.Player.Evade.triggered)
        {
            //切换到闪避状态
            playerController.SwitchState(PlayerState.Evade_Back);
            // 当前动机段数归零
            playerModel.skiilConfig.currentNormalAttackIndex = 1;
            return;
        }
        #endregion

        #region 动画是否播放结束
        if (IsAnimationEnd())
        {
            //切换到待机状态
            playerController.SwitchState(PlayerState.Idle);
            // 当前动机段数归零
            playerModel.skiilConfig.currentNormalAttackIndex = 1;
            return;
        }
        #endregion
    }
}
