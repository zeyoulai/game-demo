using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家大招开始状态
/// </summary>
public class PlayerBigSkillStartState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();

        //切换镜头
        CameraManager.INSTANCE.cm_brain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 1.25f);
        CameraManager.INSTANCE.freeLookCanmera.SetActive(false);
        playerModel.bigSkillStartShot.SetActive(true);

       //播放攻击后摇动画
       playerController.PlayAnimation("BigSkill_Start", 0.0f);
    }

    public override void Update()
    {
        base.Update();

        #region 动画是否播放结束
        if (IsAnimationEnd())
        {
            //切换到待机状态
            playerController.SwitchState(PlayerState.BigSkill);
            return;
        }
        #endregion
    }
}
