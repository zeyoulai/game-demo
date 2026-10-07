using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家180°转身状态
/// </summary>
public class PlayerTurnBackState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();

        playerController.PlayAnimation("TurnBack", 0.1f);
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

        #region 动画是否播放结束
        if (IsAnimationEnd())
        {
            playerController.SwitchState(PlayerState.Run);
            return;
        }
        #endregion
    }
}
