using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private RectTransform rect;
    private UI_SkillTree skillTree;
    private UI_TreeConnectHandler connectHandler;

    [Header("Unlock details")]
    public UI_TreeNode[] needNodes;
    public UI_TreeNode[] conflictNodes;
    public bool isUnlocked;
    public bool isLocked;

    [Header("Skill details")]
    [SerializeField] public Skill_DataSO skillData;
    [SerializeField] private string skillName;
    [SerializeField] private Image skillIcon;
    [SerializeField] private int skillCost;
    //[SerializeField] private Color skillLockedColor;
    [SerializeField] private string lockedColor = "#767676";
    private Color lastColor;

    private void Start()
    {
        if(isUnlocked == false)
            UpdateIconColor(GetColorByHex(lockedColor));
        UnlockedDefaultSkill();

    }

    public void UnlockedDefaultSkill()
    {
        GetNeededComponents();

        if (skillData.unlockedByDefault)
        {
            Unlock();
        }
    }

    private void GetNeededComponents()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        skillTree = GetComponentInParent<UI_SkillTree>(true);
        connectHandler = GetComponent<UI_TreeConnectHandler>();
    }

    public void Refund()
    {
        if(isUnlocked == false || skillData.unlockedByDefault)
            return;

        isUnlocked = false;
        isLocked = false;
        
        UpdateIconColor(GetColorByHex(lockedColor));
        skillTree.AddSkillPoints(skillData.cost);
        connectHandler.UnlockConnectionImage(false);
        //重置
        //skillData.upagradeData.upgradeType = SkillUpgradeType.None;这个重置只是重置data里面的，而不是skillhandler上面的重置
    }



    private void Unlock()
    {
        if (isUnlocked)
        {
            Debug.Log("Skill is already unlocked!");
            return;
        }

        isUnlocked = true;
        UpdateIconColor(Color.white);
        LockConflictNodes();

        skillTree.RemoveSkillPoints(skillData.cost);
        connectHandler.UnlockConnectionImage(true);

        skillTree.skillManager.GetSkillByType(skillData.skillType).SetSkillUpgrade(skillData);//按类型选择技能，并且对应成升级版本
        skillTree.skillManager.GetSkillByType(skillData.skillType).SetSkillType(skillData.skillType);//原本没有，我加上了可能更好吧
    }

    public void UnlockWithSaveData()
    {
        GetNeededComponents();
        isUnlocked = true;
        UpdateIconColor(Color.white);
        LockConflictNodes();
        connectHandler.UnlockConnectionImage(true);
    }

    public void ResetNode()
    {
        GetNeededComponents();
        isUnlocked = false;
        isLocked = false;
        UpdateIconColor(GetColorByHex(lockedColor));
        connectHandler.UnlockConnectionImage(false);
    }

    private bool CanBeUnlocked()
    {
        if (isLocked || isUnlocked)
            return false;

        if(skillTree.EnoughSkillPoints(skillData.cost) == false)
            return false;

        foreach (var node in needNodes)
        {
            if(node.isUnlocked == false)
                return false;
        }
        foreach(var node in conflictNodes)
        {
            if(node.isUnlocked)
                return false;
        }

        return true;
    }





    private void LockConflictNodes()
    {
        foreach (var node in conflictNodes)
        {
            node.isLocked = true;
            node.LockChildrenNodes();
        }
    }

    public void LockChildrenNodes()
    {
        isLocked = true;
        foreach (var node  in connectHandler.GetChildrenNodes())
        {
            node.LockChildrenNodes();
        }
    }

    private void UpdateIconColor(Color color)
    {
        if (skillIcon == null)
        {
            return;
        }
        lastColor = skillIcon.color;
        skillIcon.color = color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanBeUnlocked())
        {
            Unlock();
        }
        else if(isLocked)
        {
            ui.skillTooTip.LockedSkillEffect();
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.skillTooTip.ShowToolTip(true, rect,skillData,this);

        //if (isLocked) 这个是有效的
        //    return;

        //if (isUnlocked == false || isLocked == false)
        //    ToggleNodeHighlight(true);

        if (isUnlocked || isLocked)
            return;

        ToggleNodeHighlight(true);




    }

    private void ToggleNodeHighlight(bool highlight)
    {
        Color highlightColor = Color.white * .9f; highlightColor.a = 1;
        Color ColorToApply = highlight ? highlightColor : lastColor;
        UpdateIconColor(ColorToApply);
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        ui.skillTooTip.ShowToolTip(false, rect);
        ui.skillTooTip.StopLockedSkillEffect();
        if (isUnlocked || isLocked)
            return;

        ToggleNodeHighlight(false);

        //if (isLocked)
        //    return;

        //if (isUnlocked == false || isLocked == false)
        //    ToggleNodeHighlight(false);
    }

    private Color GetColorByHex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var color);
        return color;
    }

    private void OnDisable()
    {
        if (isLocked)
            UpdateIconColor(GetColorByHex(lockedColor));
        if(isUnlocked)
            UpdateIconColor(Color.white);
    }

    private void OnValidate()
    {
        if (skillData == null)
        {
            return;
        }
        skillName = skillData.displayName;
        skillIcon.sprite = skillData.icon;
        skillCost = skillData.cost;
        gameObject.name = "UI_TreeNode - " + skillData.displayName;
    }
}
