using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Íæ¼ÒÉÁ±Ü½áÊø×´Ì¬
/// </summary>
public class PlayerEvadeEndState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();

        #region ÅÐ¶ÏÇ°ºóÉÁ±Ü
        switch (playerModel.currentState)
        {
            case PlayerState.Evade_Front_End:
                playerController.PlayAnimation("Evade_Front_End",0.1f);
                break;
            case PlayerState.Evade_Back_End:
                playerController.PlayAnimation("Evade_Back_End", 0.1f);
                break;
        }
        #endregion
    }
    public override void Update()
    {
        base.Update();

        #region ¼ì²â´óÕÐ
        if (playerController.inputSystem.Player.BigSkill.triggered)
        {
            //ÇÐ»»µ½½øÈë´óÕÐ×´Ì¬
            playerController.SwitchState(PlayerState.BigSkillStart);
            return;
        }
        #endregion

        #region ¼ì²â¹¥»÷
        if (playerController.inputSystem.Player.Fire.triggered)
        {
            //ÇÐ»»µ½ÆÕÍ¨¹¥»÷×´Ì¬
            playerController.SwitchState(PlayerState.NormalAttack);
            return;
        }
        #endregion

        #region ¼ì²âÒÆ¶¯
        if (playerController.inputMoveVec2 != Vector2.zero)
        {
            switch (playerModel.currentState)
            {
                case PlayerState.Evade_Front_End:
                    playerController.SwitchState(PlayerState.Run);
                    break;
                case PlayerState.Evade_Back_End:
                    playerController.SwitchState(PlayerState.Walk);
                    break;
            }
            return;
        }
        #endregion

        #region ¶¯»­ÊÇ·ñ²¥·Å½áÊø
        if (IsAnimationEnd())
        {
            playerController.SwitchState(PlayerState.Idle);
            return;
        }
        #endregion
    }
}
