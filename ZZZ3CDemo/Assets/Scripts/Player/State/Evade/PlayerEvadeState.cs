using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家闪避状态
/// </summary>
public class PlayerEvadeState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();

        #region 判断前后闪避
        switch (playerModel.currentState)
        {
            case PlayerState.Evade_Front:
                playerController.PlayAnimation("Evade_Front", 0.1f);
                break;
            case PlayerState.Evade_Back:
                playerController.PlayAnimation("Evade_Back", 0.1f);
                break;
        }
        #endregion
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
            switch (playerModel.currentState)
            {
                case PlayerState.Evade_Front:
                    playerController.SwitchState(PlayerState.Evade_Front_End);
                    break;
                case PlayerState.Evade_Back:
                    playerController.SwitchState(PlayerState.Evade_Back_End);
                    break;
            }
            return;
        }
        #endregion
    }
}
