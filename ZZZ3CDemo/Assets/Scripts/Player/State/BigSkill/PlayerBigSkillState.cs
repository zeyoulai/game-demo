using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家大招收尾状态
/// </summary>
public class PlayerBigSkillState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();

        //切换镜头
        playerModel.bigSkillStartShot.SetActive(false);
        playerModel.bigSkillShot.SetActive(true);

        //播放攻击后摇动画
        playerController.PlayAnimation("BigSkill", 0f);
    }

    public override void Update()
    {
        base.Update();

        #region 动画是否播放结束
        if (IsAnimationEnd())
        {
            //切换到待机状态
            playerController.SwitchState(PlayerState.BigSkillEnd);
            return;
        }
        #endregion
    }
}
