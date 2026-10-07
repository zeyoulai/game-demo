using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家控制器
/// </summary>
public class PlayerController : SingleMomoBase<PlayerController>,IStateMachineOwner
{
    //输入系统
    [HideInInspector]public InputSystem inputSystem;
    //玩家移动输入
    public Vector2 inputMoveVec2;

    //玩家模型
    public PlayerModel playerModel;

    //转向速度
    public float rotationSpeed = 8f;

    // 闪避计时器
    private float evadeTimer = 1;

    //状态机
    private StateMachine stateMachine;

    //玩家配置信息
    public PlayerConfig playerConfig;

    //配队
    private List<PlayerModel> controllableModels;

    //当前控制的角色下标
    private int currentModelIndex;

    protected override void Awake()
    {
        base.Awake();

        stateMachine = new StateMachine(this);
        inputSystem= new InputSystem();

        controllableModels = new List<PlayerModel>();

        #region 生产角色模型
        for (int i = 0; i < playerConfig.models.Length; i++) 
        {
            GameObject modle = Instantiate(playerConfig.models[i], transform);
            controllableModels.Add(modle.GetComponent<PlayerModel>());
            controllableModels[i].gameObject.SetActive(false);
        }
        #endregion

        #region 操控第一个角色
        currentModelIndex = 0;
        controllableModels[currentModelIndex].gameObject.SetActive(true);
        playerModel = controllableModels[currentModelIndex];
        #endregion
    }

    private void Start()
    {
        //锁定光标
        LockMouse();
        //切换到待机状态
        SwitchState(PlayerState.Idle);
    }

    /// <summary>
    /// 切换状态
    /// </summary>
    /// <param name="playerState">状态</param>
    public void SwitchState(PlayerState playerState)
    {
        playerModel.currentState= playerState;
        switch (playerState)
        {
            case PlayerState.Idle:
            case PlayerState.Idle_AFK:
                stateMachine.EnterState<PlayerIdleState>(true);
                break;
            case PlayerState.Walk:
            case PlayerState.Run:
                stateMachine.EnterState<PlayerRunState>(true);
                break;
            case PlayerState.RunEnd:
                stateMachine.EnterState<PlayerRunEndState>();
                break;
            case PlayerState.TurnBack:
                stateMachine.EnterState<PlayerTurnBackState>();
                break;
            case PlayerState.Evade_Front:
            case PlayerState.Evade_Back:
                if (evadeTimer !=1)
                {
                    return;
                }
                stateMachine.EnterState<PlayerEvadeState>();
                evadeTimer = 0f;
                break;
            case PlayerState.Evade_Front_End:
            case PlayerState.Evade_Back_End:
                stateMachine.EnterState<PlayerEvadeEndState>();
                break;
            case PlayerState.NormalAttack:
                stateMachine.EnterState<PlayerNormalAttackState>(true);
                break;
            case PlayerState.NormalAttackEnd:
                stateMachine.EnterState<PlayerNormalAttackEndState>();
                break;
            case PlayerState.BigSkillStart:
                stateMachine.EnterState<PlayerBigSkillStartState>();
                break;
            case PlayerState.BigSkill:
                stateMachine.EnterState<PlayerBigSkillState>();
                break;
            case PlayerState.BigSkillEnd:
                stateMachine.EnterState<PlayerBigSkillEndState>();
                break;
            case PlayerState.SwitchInNormal:
                stateMachine.EnterState<PlayerSwitchInNormalState>();
                break;
        }
    }

    /// <summary>
    /// 切换到下一个模型
    /// </summary>
    public void SwitchNextModel()
    {
        //刷新状态机
        stateMachine.Clear();
        //退出当前模型
        playerModel.Exit();
        #region 控制下一个模型
        currentModelIndex++;
        if (currentModelIndex >= controllableModels.Count) 
        {
            currentModelIndex = 0;
        }
        PlayerModel nextModel = controllableModels[currentModelIndex];
        nextModel.gameObject.SetActive(true);

        Vector3 prevPos = playerModel.transform.position;
        Quaternion prevRot = playerModel.transform.rotation;

        playerModel = nextModel;
        #endregion
        //进入下一个模型
        playerModel.Enter(prevPos, prevRot);
        //切换到入场状态
        SwitchState(PlayerState.SwitchInNormal);
    }

    /// <summary>
    /// 切换到上一个模型
    /// </summary>
    public void SwitchLastModel()
    {
        //刷新状态机
        stateMachine.Clear();
        //退出当前模型
        playerModel.Exit();
        #region 控制下一个模型
        currentModelIndex--;
        if (currentModelIndex < 0)
        {
            currentModelIndex = controllableModels.Count - 1;
        }
        PlayerModel nextModel = controllableModels[currentModelIndex];
        nextModel.gameObject.SetActive(true);

        Vector3 prevPos = playerModel.transform.position;
        Quaternion prevRot = playerModel.transform.rotation;

        playerModel = nextModel;
        #endregion
        //进入下一个模型
        playerModel.Enter(prevPos, prevRot);
        //切换到入场状态
        SwitchState(PlayerState.SwitchInNormal);
    }
    

    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="fixedTransitionDurarion">过渡时间</param>
    public void PlayAnimation(string animationName,float fixedTransitionDurarion = 0.25f)
    {
        playerModel.animator.CrossFadeInFixedTime(animationName, fixedTransitionDurarion);
    }
    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="animationName">动画名称</param>
    /// <param name="fixedTransitionDurarion">过渡时间</param>
    /// <param name="fixedTimeOffset">动画起始播放偏移</param>
    public void PlayAnimation(string animationName, float fixedTransitionDurarion = 0.25f, float fixedTimeOffset = 0f)
    {
        playerModel.animator.CrossFadeInFixedTime(animationName, fixedTransitionDurarion, 0, fixedTimeOffset);
    }

    private void FixedUpdate()
    {
        //更新玩家的移动输入
        inputMoveVec2 = inputSystem.Player.Move.ReadValue<Vector2>().normalized;
        //恢复闪避计时器
        if (evadeTimer < 1f) 
        {
            evadeTimer += Time.deltaTime;
            if (evadeTimer > 1f) 
            {
                evadeTimer = 1f;
            }
        }
    }

    private void LockMouse()
    {
        //将光标锁定在屏幕中间
        Cursor.lockState = CursorLockMode.Locked;
        //隐藏光标
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputSystem.Enable();
    }
    private void OnDisable()
    {
        inputSystem.Disable();
    }
}
