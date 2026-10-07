using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    private Player player;
    private Inventory_Player inventory;
    private UI_SkillSlot[] skillSlots;

    [SerializeField] private RectTransform healthRect;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Quick Item Slots")]
    [SerializeField] private float yOffsetQuickItemParent = 150;
    [SerializeField] private Transform quickItemOptionsParent;
    private UI_QuickItemSlotOption[] quickItemOptions;
    private UI_QuickItemSlot[] quickItemSlots;
    [Header("Close Button")]
    [SerializeField] private Button closeBTN;
    private void Start()
    {
        quickItemSlots = GetComponentsInChildren<UI_QuickItemSlot>();

        player = FindFirstObjectByType<Player>();
        player.health.OnHealthUpdate += UpdateHealthBar;

        inventory = player.inventory;
        inventory.OnInventoryChange += UpdateQuickSlotsUI;
        inventory.OnQuickSlotUsed += PlayQuickSlotFeedback;
        skillSlots = GetComponentsInChildren<UI_SkillSlot>(true);
        CacheSkillSlotDefaultStates();

        UpdateQuickSlotsUI();
        UpdateHealthBar();
    }

    public void PlayQuickSlotFeedback(int slotNumber) => quickItemSlots[slotNumber].SimulateButtonFeedback(); 

    public void UpdateQuickSlotsUI()
    {
        Inventory_Item[] quickItems = inventory.quickItems;
        int slotCount = Mathf.Min(quickItems.Length, quickItemSlots.Length);

        for (int i = 0; i < slotCount; i++)
        {
            quickItemSlots[i].UpdateQuickSlotUI(quickItems[i]); 
        }

    }

    public void OpenQuickItemOptions(UI_QuickItemSlot quickItemSlot,RectTransform targetRect)
    {
        closeBTN?.gameObject.SetActive(true);


        if (quickItemOptions == null)
        {
            quickItemOptions = quickItemOptionsParent.GetComponentsInChildren<UI_QuickItemSlotOption>(true);

        }

        List<Inventory_Item> consumables = inventory.itemList.FindAll(item => item.itemData.itemType == ItemType.Consumable);

        for (int i = 0; i < quickItemOptions.Length; i++)
        {
            if (i < consumables.Count)
            {
                quickItemOptions[i].gameObject.SetActive(true);
                quickItemOptions[i].SetupOption(quickItemSlot, consumables[i]);
            }
            else
                quickItemOptions[i].gameObject.SetActive(false);
        }

        quickItemOptionsParent.position = targetRect.position + Vector3.up * yOffsetQuickItemParent;

    }

    public void HideQuickItemOptions() => quickItemOptionsParent.position = new Vector3(0, 9999);



    public UI_SkillSlot GetSkillSlot(SkillType skillType)
    {
        if (skillSlots == null)
            skillSlots = GetComponentsInChildren<UI_SkillSlot>(true);

        foreach (var slot in skillSlots)
        {
            if (slot.skillType == skillType)
            {
                slot.gameObject.SetActive(true);
                return slot;
            }

        }
        return null;
    }

    public void ResetSkillSlots()
    {
        if (skillSlots == null)
            skillSlots = GetComponentsInChildren<UI_SkillSlot>(true);

        CacheSkillSlotDefaultStates();

        foreach (var slot in skillSlots)
        {
            if (slot == null)
                continue;

            slot.ClearSkillSlot();
        }
    }

    private void CacheSkillSlotDefaultStates()
    {
        if (skillSlots == null)
            return;

        foreach (var slot in skillSlots)
        {
            if (slot == null)
                continue;

            slot.CacheDefaultState();
        }
    }

    public void UpdateHealthBar()
    {
        float currentHealth = Mathf.RoundToInt(player.health.GetCurrentHealth());
        float maxHealth = player.stats.GetMaxHealth();

        float sizeDiffrence = Mathf.Abs(maxHealth-healthRect.sizeDelta.x);
        if(sizeDiffrence > .1f)
        {
            healthRect.sizeDelta = new Vector2(Mathf.Clamp(maxHealth * Mathf.Log(100, maxHealth),150,875), healthRect.sizeDelta.y);
        }

        healthText.text = currentHealth + "/" + maxHealth;
        healthSlider.value = player.health.GetHealthPercent();
    }
}
