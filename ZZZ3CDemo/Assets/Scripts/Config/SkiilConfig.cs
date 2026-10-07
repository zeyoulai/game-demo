using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能配置
/// </summary>
[CreateAssetMenu(menuName = "Config/Skill")] 
public class SkiilConfig : ScriptableObject
{
    //当前普通攻击所处的段数
    [HideInInspector]public int currentNormalAttackIndex = 1;

    //普通攻击每段的伤害倍率
    public float[] normalAttackDamageMultiple;
}
