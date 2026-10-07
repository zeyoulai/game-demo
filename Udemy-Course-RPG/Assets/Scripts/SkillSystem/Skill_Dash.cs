using UnityEngine;

public class Skill_Dash : Skill_Base
{

    private Player player;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponentInParent<Player>();
    }

    //感觉应该做成虚函数
    public void OnStartEffect()
    {
        //if(Unlocked(SkillUpgradeType.Dash_CloneOnStart) || Unlocked(SkillUpgradeType.Dash_CloneOnStartAndArrival)||Unlocked(SkillUpgradeType.Dash))
        //    player.vfx.DoImageEchoEffect(player.dashDuration);


        if (Unlocked(SkillUpgradeType.Dash_CloneOnStart) || Unlocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
            CreateClone();
        

        if (Unlocked(SkillUpgradeType.Dash_ShardOnStart) || Unlocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
            CreateShard();
    }

    public void OnEndEffect()
    {
        if(Unlocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
            CreateClone();
        if(Unlocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
            CreateShard();
    }

    private void CreateShard()
    {

        skillManager.shard.CreateRawShard();
    }

    private void CreateClone()
    {
        Debug.Log("Create time clone");

        //skill manager clone create clone
        skillManager.timeEcho.CreateTimeEcho();
    }

}
