using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModelFoot
{
    Left, Right
}
public class PlayerModel : MonoBehaviour
{
    //动画控制器
    [HideInInspector] public Animator animator;
    //玩家当前状态
    public PlayerState currentState;
    //角色控制器
    [HideInInspector] public CharacterController characterController;
    //动画信息
    private AnimatorStateInfo stateInfo;
    //重力
    public float gravity = -9.8f;
    //技能配置文件
    public SkiilConfig skiilConfig;
    //大招Start镜头
    public GameObject bigSkillStartShot;
    //大招镜头
    public GameObject bigSkillShot;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    #region 动画状态
    public ModelFoot foot = ModelFoot.Left;

    /// <summary>
    /// 模型入场
    /// </summary>
    public void Enter(Vector3 pos, Quaternion rot)
    {
        //强行移除退场逻辑
        MonoManager.INSTANCE.RemoveUpdateAction(OnExit);

        #region 设置角色出场位置
        //计算向右的向量
        Vector3 rightDirection = rot * Vector3.right;
        //向右偏移0.8个单位
        pos += rightDirection * 0.8f;
        //计算向后的向量
        Vector3 backDirection = rot * Vector3.back;
        //向后偏移3个单位
        pos += backDirection * 4f;

        characterController.Move(pos - transform.position);
        transform.rotation = rot;
        #endregion
    }

    /// <summary>
    /// 模型退场
    /// </summary>
    public void Exit() 
    {
        animator.CrossFade("SwitchOut_Normal", 0.1f);
        MonoManager.INSTANCE.AddUpdateAction(OnExit);
    }

    /// <summary>
    /// 退场逻辑
    /// </summary>
    public void OnExit()
    {
        #region 检测动画是否播放结束
        if (IsAnimationEnd())
        {
            gameObject.SetActive(false);
            MonoManager.INSTANCE.RemoveUpdateAction(OnExit);
        }
        #endregion
    }

    /// <summary>
    /// 判断动画是否结束
    /// </summary>
    public bool IsAnimationEnd()
    {
        //刷新动画状态
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0);
    }

    /// <summary>
    /// 迈出左脚
    /// </summary>
    public void SetOutLeftFoot()
    {
        foot = ModelFoot.Left;
    }

    /// <summary>
    /// 迈出右脚
    /// </summary>
    public void SetOutRightFoot()
    {
        foot = ModelFoot.Right;
    }

    private void OnDisable()
    {
        //重置普通攻击段数
        skiilConfig.currentNormalAttackIndex = 1;
    }
    #endregion
}
