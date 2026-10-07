using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    /// <summary>
    /// ´ý»ú×´Ì¬
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        switch (playerModel.currentState) 
        {
            case PlayerState.Idle:
                playerController.PlayAnimation("Idle", 0.25f);
                break;
            case PlayerState.Idle_AFK:
                playerController.PlayAnimation("Idle_AFK", 0.25f);
                break;
        }  
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
            //ÇÐ»»µ½ÒÆ¶¯×´Ì¬
            playerController.SwitchState(PlayerState.Walk);
            return;
        }
        #endregion

        #region ¼ì²âÉÁ±Ü
        if (playerController.inputSystem.Player.Evade.triggered)
        {
            //ÇÐ»»µ½ÉÁ±Ü×´Ì¬
            playerController.SwitchState(PlayerState.Evade_Back);
            return;
        }
        #endregion

        switch (playerModel.currentState)
        {
            case PlayerState.Idle:
                #region ¼ì²â¹Ò»ú
                if (statePlayTime > 3) 
                {
                    //ÇÐ»»µ½¹Ò»ú×´Ì¬
                    playerController.SwitchState(PlayerState.Idle_AFK);
                    return;
                }
                #endregion
                break;
            case PlayerState.Idle_AFK:
                #region ¼ì²â¹Ò»ú¶¯»­½áÊø
                if (IsAnimationEnd()) 
                {
                    //ÇÐ»»µ½´ý»ú×´Ì¬
                    playerController.SwitchState(PlayerState.Idle);
                    return;
                }
                #endregion
                break;
        }

    }
}
