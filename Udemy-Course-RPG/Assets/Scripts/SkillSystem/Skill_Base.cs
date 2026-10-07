using UnityEngine;

public class Skill_Base : MonoBehaviour
{
    public Player_SkillManager skillManager { get; private set; }
    public Player player { get; private set; }

    [Header("Setup Sword Data")]
    public DamageScaleData damageScaleData;
    [Space]
    [Header("General details")]
    [SerializeField] protected SkillType skillType;
    [SerializeField] protected SkillUpgradeType upgradeType;
    [SerializeField] protected float cooldown;
    private float lastTimeUsed;

    protected virtual void Awake()
    {
        skillManager = GetComponentInParent<Player_SkillManager>();
        player = GetComponentInParent<Player>();
        lastTimeUsed = lastTimeUsed - cooldown;

        //damageScaleData = new DamageScaleData();//默认值，防止报错 ,这个导致这个技能只能使用默认值了
    }

    public virtual void TryUseSkill()
    {

    }

    public void SetSkillUpgrade(Skill_DataSO skillData)
    {
        UpagradeData upgrade = skillData.upagradeData;
        this.upgradeType = upgrade.upgradeType;
        this.cooldown = upgrade.cooldown;

        damageScaleData = upgrade.damageScaleData;

        player.ui.inGameUI.GetSkillSlot(skillType).SetupSkillSlot(skillData);
        ReSetCooldown();
    }

    public void SetSkillType(SkillType type)
    {
        this.skillType = type;
    }

    public void SetSkillType(SkillType type, SkillUpgradeType upgradeType)
    {
        this.skillType = type;
        this.upgradeType = upgradeType;
    }

    public virtual bool CanUseSkill()
    {
        if (SkillUpgradeType.None == upgradeType)
            return false;

        if (OnCooldown())
        {
            Debug.Log("On Cooldown");
            return false;
        }

        return true;
    }

    protected bool Unlocked(SkillUpgradeType upgradeToCheck) => upgradeType == upgradeToCheck;

    public SkillUpgradeType GetUpgrade() => upgradeType;
    public SkillType GetSkillType() => skillType;

    protected bool OnCooldown() => Time.time < lastTimeUsed + cooldown;
    public void SetSkillOnCooldown()
    {
        player.ui.inGameUI.GetSkillSlot(skillType).StartCooldown(cooldown);
        lastTimeUsed = Time.time;
    }
        
    public void ReduceCooldownBy(float cooldownReduction) => lastTimeUsed = lastTimeUsed + cooldownReduction;
    public void ReSetCooldown()
    {
        player.ui.inGameUI.GetSkillSlot(skillType).ResetCooldown();
        lastTimeUsed = Time.time - cooldown;

    }
}
