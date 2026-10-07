using System.Linq;
using TMPro;
using UnityEngine;

public class UI_SkillTree : MonoBehaviour,ISaveable
{
    public int skillPoints;
    [SerializeField] private TextMeshProUGUI skillPointsText;
    [SerializeField] private UI_TreeConnectHandler[] parentNodes;
    private UI_TreeNode[] allTreeNodes;
    private int startingSkillPoints;
    private bool startingSkillPointsCached;
    public Player_SkillManager skillManager { get; private set; }

    private void Start()
    {
        CacheStartingSkillPoints();
        UpdateAllConnections();
        UpdateSkillPointsUI();
    }

    private void CacheStartingSkillPoints()
    {
        if (startingSkillPointsCached)
            return;

        startingSkillPoints = skillPoints;
        startingSkillPointsCached = true;
    }

    private void UpdateSkillPointsUI()
    {
        skillPointsText.text = skillPoints.ToString();
    }

    public void UnlockDefaultSkills()
    {
        CacheStartingSkillPoints();
        allTreeNodes = GetComponentsInChildren<UI_TreeNode>(true);
        skillManager = FindAnyObjectByType<Player_SkillManager>();

        foreach (var node in allTreeNodes)
        {
            node.UnlockedDefaultSkill();
        }

    }

    [ContextMenu("Reset Skill Tree")]
    public void RefundAllSkills()
    {
        if (allTreeNodes == null)
            allTreeNodes = GetComponentsInChildren<UI_TreeNode>(true);

        if (skillManager == null)
            skillManager = FindAnyObjectByType<Player_SkillManager>();

        int pointsToRefund = 0;

        foreach (var node in allTreeNodes)
        {
            if (node.skillData != null && node.isUnlocked && node.skillData.unlockedByDefault == false)
                pointsToRefund += node.skillData.cost;
        }

        ResetSkillTree();
        skillPoints += pointsToRefund;
        UnlockDefaultSkills();
        UpdateAllConnections();
        UpdateSkillPointsUI();
    }

    public bool HasRefundableSkills()
    {
        if (allTreeNodes == null)
            allTreeNodes = GetComponentsInChildren<UI_TreeNode>(true);

        foreach (var node in allTreeNodes)
        {
            if (node.skillData != null && node.isUnlocked && node.skillData.unlockedByDefault == false)
                return true;
        }

        return false;
    }

    public bool EnoughSkillPoints(int cost) => skillPoints >= cost;

    public void RemoveSkillPoints(int cost)
    {

        skillPoints -= cost;
        UpdateSkillPointsUI();
    }

    public void AddSkillPoints(int points)
    {
        skillPoints += points;
        UpdateSkillPointsUI();

    }

    [ContextMenu("Update All Connections")]
    public void UpdateAllConnections()
    {
        foreach (var node in parentNodes)
        {
            node.UpdateAllConnections();
        }
    }

    public void LoadData(GameData data)
    {
        CacheStartingSkillPoints();

        if (allTreeNodes == null)
            allTreeNodes = GetComponentsInChildren<UI_TreeNode>(true);

        if (skillManager == null)
            skillManager = FindAnyObjectByType<Player_SkillManager>();

        ResetSkillTree();

        bool hasSkillTreeSaveData = data.hasSkillTreeData || (data.skillTreeUI != null && data.skillTreeUI.Count > 0);
        skillPoints = hasSkillTreeSaveData ? data.skillPoints : startingSkillPoints;

        if (hasSkillTreeSaveData)
        {
            foreach (var node in allTreeNodes)
            {
                string skillName = node.skillData.displayName;

                if (data.skillTreeUI.TryGetValue(skillName, out bool unlocked) && unlocked)
                    node.UnlockWithSaveData();
            }
        }
        else
        {
            foreach (var node in allTreeNodes)
            {
                node.UnlockedDefaultSkill();
            }
        }

        if (hasSkillTreeSaveData && skillManager != null && data.skillUpgrades != null)
        {
            foreach (var skill in skillManager.allSkills)
            {
                if (data.skillUpgrades.TryGetValue(skill.GetSkillType(), out SkillUpgradeType upgradeType))
                {
                    var upgradeNode = allTreeNodes.FirstOrDefault(node => node.skillData.upagradeData.upgradeType == upgradeType);

                    if (upgradeNode != null)
                        skill.SetSkillUpgrade(upgradeNode.skillData);
                }
            }
        }

        UpdateSkillPointsUI();
    }

    public void SaveData(ref GameData data)
    {
        if (allTreeNodes == null)
            allTreeNodes = GetComponentsInChildren<UI_TreeNode>(true);

        if (skillManager == null)
            skillManager = FindAnyObjectByType<Player_SkillManager>();

        data.hasSkillTreeData = true;
        data.skillPoints = skillPoints;
        data.skillTreeUI.Clear(); 
        data.skillUpgrades.Clear();
        foreach (var node in allTreeNodes)
        {
            string skillName = node.skillData.displayName;
            data.skillTreeUI[skillName] = node.isUnlocked;
        }

        if (skillManager == null)
            return;

        foreach (var skill in skillManager.allSkills)
        {
            data.skillUpgrades[skill.GetSkillType()] = skill.GetUpgrade();
        }
    }

    private void ResetSkillTree()
    {
        foreach (var node in allTreeNodes)
            node.ResetNode();

        skillManager?.WashAllSkills();
        UI.instance?.inGameUI?.ResetSkillSlots();
    }
}
