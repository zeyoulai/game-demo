using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Íæ¼ÒÒÆ¶¯×´Ì¬
/// </summary>
public class PlayerRunState : PlayerStateBase
{
    private Camera mainCamera;

    public override void Enter()
    {
        base.Enter();

        mainCamera = Camera.main;

        //ÅÐ¶ÏÒÆ¶¯×´Ì¬
        switch (playerModel.currentState)
        {
            case PlayerState.Walk:
                #region Âõ³ö×óÓÒ½ÅµÄÅÐ¶Ï
                switch (playerModel.foot)
                {
                    case ModelFoot.Left:
                        playerController.PlayAnimation("Walk", 0.125f, 0.5f);
                        playerModel.foot = ModelFoot.Right;
                        break;
                    case ModelFoot.Right:
                        playerController.PlayAnimation("Walk", 0.125f, 0.0f);
                        playerModel.foot = ModelFoot.Left;
                        break;
                }
                #endregion
                break;
            case PlayerState.Run:
                #region Âõ³ö×óÓÒ½ÅµÄÅÐ¶Ï
                switch (playerModel.foot)
                {
                    case ModelFoot.Left:
                        playerController.PlayAnimation("Run", 0.125f, 0.5f);
                        playerModel.foot = ModelFoot.Right;
                        break;
                    case ModelFoot.Right:
                        playerController.PlayAnimation("Run", 0.125f, 0.0f);
                        playerModel.foot = ModelFoot.Left;
                        break;
                }
                #endregion
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

        #region ¼ì²âÉÁ±Ü
        if (playerController.inputSystem.Player.Evade.triggered)
        {
            //ÇÐ»»µ½ÉÁ±Ü×´Ì¬
            playerController.SwitchState(PlayerState.Evade_Front);
            return;
        }
        #endregion

        #region ¼ì²â´ý»ú
        if (playerController.inputMoveVec2 == Vector2.zero)
        {
            playerController.SwitchState(PlayerState.RunEnd);
            return;
        }
        #endregion
        else
        {
            #region ´¦ÀíÒÆ¶¯·½Ïò
            Vector3 inputMoveVec3 = new Vector3(playerController.inputMoveVec2.x, 0, playerController.inputMoveVec2.y);
            //»ñÈ¡Ïà»úµÄÐý×ªÖáY
            float cameraAxisY = mainCamera.transform.rotation.eulerAngles.y;
            //ËÄÔªÊý ¡Á ÏòÁ¿
            Vector3 targetDic = Quaternion.Euler(0, cameraAxisY, 0) * inputMoveVec3;
            Quaternion targetQua = Quaternion.LookRotation(targetDic);
            //¼ÆËãÐý×ª½Ç¶È
            float angles = Mathf.Abs(targetQua.eulerAngles.y - playerModel.transform.eulerAngles.y);
            if (angles > 145f && angles < 215f && playerModel.currentState == PlayerState.Run)  
            {
                //ÇÐ»»×ªÉí×´Ì¬
                playerController.SwitchState(PlayerState.TurnBack);
            }
            else
            {
                playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, targetQua, Time.deltaTime * playerController.rotationSpeed);
            }
            #endregion
        }

        #region ¼ì²âÂýÅÜÉý¼¶
        if (playerModel.currentState == PlayerState.Walk && statePlayTime > 3f) 
        {
            //ÇÐ»»µ½¼²ÅÜ×´Ì¬
            playerController.SwitchState(PlayerState.Run);
            return;
        }
        #endregion
    }
}
