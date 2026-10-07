using TMPro;
using UnityEngine;

public class UI_Merchant : MonoBehaviour
{
    private Inventory_Player inventory;
    private Inventory_Merchant merchant;

    [SerializeField] private TextMeshProUGUI goldText;
    [Space]
    [SerializeField] private UI_ItemSlotParent merchantSlots;
    [SerializeField] private UI_ItemSlotParent inventorySlots;
    [SerializeField] private UI_EquipSlotParent equipSlots;

    public void SetupMerchantUI(Inventory_Merchant merchant, Inventory_Player inventory)
    {
        Unsubscribe();

        this.merchant = merchant;
        this.inventory = inventory;

        if (this.inventory != null)
            this.inventory.OnInventoryChange += UpdateSlotUI;

        if (this.merchant != null)
            this.merchant.OnInventoryChange += UpdateSlotUI;

        UI_MerchantSlot[] slotUIs = GetComponentsInChildren<UI_MerchantSlot>(true);
        foreach (var slot in slotUIs)
        {
            slot.SetupMerchantUI(merchant);
        }

        UpdateSlotUI();
    }

    private void UpdateSlotUI()
    {

        if (inventory == null || merchant == null) return;
        merchantSlots.UpdateSlots(merchant.itemList);
        inventorySlots.UpdateSlots(inventory.itemList);
        equipSlots.UpdateEquipmentSlots(inventory.equipList);
        goldText.text = inventory.gold.ToString("N0")+"g.";
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (inventory != null)
            inventory.OnInventoryChange -= UpdateSlotUI;

        if (merchant != null)
            merchant.OnInventoryChange -= UpdateSlotUI;
    }
}
