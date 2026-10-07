using System.Collections;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UI_SkillToolTip : UI_ToolTip
{
    private UI ui;
    private UI_SkillTree skillTree;

    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] protected TextMeshProUGUI skillDescription;
    [SerializeField] private TextMeshProUGUI skillCooldown;
    [SerializeField] public TextMeshProUGUI skillRequirements;


    [Space]
    [SerializeField] private string metConditionHex;
    [SerializeField] private string notMetConditionHex;
    [SerializeField] private string impotantInfoHex;
    [SerializeField] private Color exampleColor;
    [SerializeField] private string lockedSkillText = "You've choosen your way - this skill is now locked.";

    private Coroutine textEffectCo;
    protected override void Awake()
    {
        base.Awake();
        ui = GetComponentInParent<UI>();
        skillTree = ui.GetComponentInChildren<UI_SkillTree>(true);
    }

    public override void ShowToolTip(bool show, RectTransform targetRect)
    {
        base.ShowToolTip(show, targetRect);



    }

    public void ShowToolTip(bool show ,RectTransform targetRect,Skill_DataSO skillData,UI_TreeNode node,bool down = false)
    {
        base.ShowToolTip(show, targetRect, down);
        if (show == false)
        {
            return;
        }

        skillName.text = skillData.displayName;
        skillDescription.text = skillData.description;
        skillCooldown.text ="Cooldown:" + skillData.upagradeData.cooldown + " s.";
        if(node == null)
        {
            skillRequirements.text = "";
            return;
        }

        string skillLockedText = GetColoredText(impotantInfoHex, lockedSkillText);
        string requirements = node.isLocked ? skillLockedText: GetRequirements(node.skillData.cost, node.needNodes, node.conflictNodes);

        skillRequirements.text = requirements;


    }

    public void LockedSkillEffect()
    {
        StopLockedSkillEffect();
        textEffectCo = StartCoroutine(TextBlinkEffectCo(skillRequirements,.15f,3));
    }

    public void StopLockedSkillEffect()
    {
        if (textEffectCo != null)
            StopCoroutine(textEffectCo);
    }

    private IEnumerator TextBlinkEffectCo(TextMeshProUGUI text, float blinkInterval, int blinkCount)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            text.text = GetColoredText(notMetConditionHex, lockedSkillText);
            yield return new WaitForSeconds(blinkInterval);
            text.text = GetColoredText(impotantInfoHex, lockedSkillText);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private string GetRequirements(int skillCost, UI_TreeNode[] neededNodes, UI_TreeNode[] conflictNodes)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Requirements:");

        string costColor = skillTree.EnoughSkillPoints(skillCost) ? metConditionHex : notMetConditionHex;
        string costText = $"- {skillCost} skill point(s)";
        string finalCostText = GetColoredText(costColor, costText);
        sb.AppendLine(finalCostText);
        foreach (var node in neededNodes)
        {
            if (node == null) continue;
            string nodeColor = node.isUnlocked ? metConditionHex : notMetConditionHex;
            string nodeText = $"- {node.skillData.displayName}";
            string finalNodeText = GetColoredText(nodeColor, nodeText);
            sb.AppendLine(finalNodeText);

        }

        if (conflictNodes.Length <= 0)
            return sb.ToString();

        sb.AppendLine();
        sb.AppendLine(GetColoredText(impotantInfoHex,"Locks out:"));
        foreach (var node in conflictNodes)
        {

            if(node == null) continue;
            string nodeText = $"- {node.skillData.displayName}";
            string finalNodeText = GetColoredText(impotantInfoHex, nodeText);
            sb.AppendLine(finalNodeText);

        }

        return sb.ToString();

    }

 


}
