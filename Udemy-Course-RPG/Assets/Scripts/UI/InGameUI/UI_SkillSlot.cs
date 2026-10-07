using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SkillSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private UI ui;
    private Image skillIcon;
    private RectTransform rect;
    private Button button;
    private Sprite defaultSkillIcon;
    private Color defaultSkillIconColor;
    private Color defaultCooldownColor;
    private float defaultCooldownFillAmount;
    private string defaultInputKeyText;
    private bool defaultConflictSlotActive;
    private bool defaultSlotActive;
    private bool defaultStateCached;

    private Skill_DataSO skillData;

    public SkillType skillType;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private string inputKeyName;
    [SerializeField] private TextMeshProUGUI inputKeyText;

    [SerializeField] private GameObject conflictSlot;
    private void Awake()
    {
        EnsureComponents();
        CacheDefaultState();
    }

    private void OnValidate()
    {
        gameObject.name = "UI_SkillSlot - "+skillType.ToString();
    }

    public void SetupSkillSlot(Skill_DataSO selectedSkill)
    {
        EnsureComponents();
        CacheDefaultState();

        if (selectedSkill == null)
        {
            ClearSkillSlot();
            return;
        }

        gameObject.SetActive(true);
        this.skillData = selectedSkill;

        Color color = Color.black; color.a = .85f;
        if (cooldownImage != null)
            cooldownImage.color = color;

        if (inputKeyText != null)
            inputKeyText.text = inputKeyName;

        if (skillIcon != null)
        {
            skillIcon.color = Color.white;
            skillIcon.sprite = selectedSkill.icon;
        }

        if(conflictSlot != null)
        {
            conflictSlot.SetActive(false);
        }
    }

    public void ClearSkillSlot()
    {
        EnsureComponents();
        CacheDefaultState();

        skillData = null;

        if (skillIcon != null)
        {
            skillIcon.sprite = defaultSkillIcon;
            skillIcon.color = defaultSkillIconColor;
        }

        if (inputKeyText != null)
            inputKeyText.text = defaultInputKeyText;

        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = defaultCooldownFillAmount;
            cooldownImage.color = defaultCooldownColor;
        }

        if (conflictSlot != null)
            conflictSlot.SetActive(defaultConflictSlotActive);

        gameObject.SetActive(defaultSlotActive);
    }

    public void StartCooldown(float cooldown)
    {
        if (cooldownImage == null)
            return;

        cooldownImage.fillAmount = 1;
        StartCoroutine(CooldownCo(cooldown));
    }

    public void ResetCooldown()
    {
        if (cooldownImage != null)
            cooldownImage.fillAmount=0;
    }

    private IEnumerator CooldownCo(float duration)
    {
        float timePassed = 0;

        while (timePassed < duration)
        {
            timePassed = timePassed + Time.deltaTime;
            if (cooldownImage != null)
                cooldownImage.fillAmount = 1f - (timePassed / duration);
            yield return null;
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(skillData == null) return;
        Debug.Log("rect:" + rect.position);

        ui.skillTooTip.ShowToolTip(true, rect, skillData,null,true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.skillTooTip.ShowToolTip(false,null);
    }

    public void CacheDefaultState()
    {
        EnsureComponents();

        if (defaultStateCached)
            return;

        if (skillIcon != null)
        {
            defaultSkillIcon = skillIcon.sprite;
            defaultSkillIconColor = skillIcon.color;
        }

        if (cooldownImage != null)
        {
            defaultCooldownColor = cooldownImage.color;
            defaultCooldownFillAmount = cooldownImage.fillAmount;
        }

        if (inputKeyText != null)
            defaultInputKeyText = inputKeyText.text;

        defaultConflictSlotActive = conflictSlot != null && conflictSlot.activeSelf;
        defaultSlotActive = gameObject.activeSelf;
        defaultStateCached = true;
    }

    private void EnsureComponents()
    {
        if (ui == null)
            ui = GetComponentInParent<UI>();

        if (button == null)
            button = GetComponent<Button>();

        if (skillIcon == null)
            skillIcon = GetComponent<Image>();

        if (rect == null)
            rect = GetComponent<RectTransform>();
    }
}
