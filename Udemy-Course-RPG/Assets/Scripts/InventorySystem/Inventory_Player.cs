
using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : Inventory_Base
{
    public event Action<int> OnQuickSlotUsed;

    public Inventory_Storage storage { get; private set; }
    public List<Inventory_EquipmentSlot> equipList;

    [Header("Quick Item Slots")]
    public Inventory_Item[] quickItems = new Inventory_Item[2];

    [Header("Gold Info")]
    public int gold = 10000;


    protected override void Awake()
    {
        base.Awake();

        storage = FindAnyObjectByType<Inventory_Storage>();
    }

    public void SetQuickItemInSlot(int slotNumber, Inventory_Item itemToSet)
    {
        quickItems[slotNumber - 1] = itemToSet;
        TriggerUpdateUI();
    }
    public void TryUseQuickItemInSlot(int passedSlotNumber)
    {
        int slotNumber = passedSlotNumber - 1;
        var itemToUse = quickItems[slotNumber];


        if (itemToUse == null) return;

        TryUseItem(itemToUse);

        if (FindItem(itemToUse) == null)
        {
            quickItems[slotNumber] = FindSameItem(itemToUse);

        }
        TriggerUpdateUI();
        OnQuickSlotUsed?.Invoke(slotNumber);
    }

    public void TryEquipItem(Inventory_Item item)
    {
        Inventory_Item inventoryItem = FindItem(item);
        var matchingSlots = equipList.FindAll(slot => slot.slotType == item.itemData.itemType);

        foreach (var slot in matchingSlots)
        {
            if (slot.HasItem() == false)
            {
                EquipItem(inventoryItem, slot);
                return;
            }
        }

        var slotToReplace = matchingSlots[0];
        var itemToUnequip = slotToReplace.equipedItem;

        UnequipItem(itemToUnequip, slotToReplace != null);
        EquipItem(inventoryItem, slotToReplace);
    }

    private void EquipItem(Inventory_Item itemToEquip, Inventory_EquipmentSlot slot)
    {
        float savedHealthPercent = player.health.GetHealthPercent();

        slot.equipedItem = itemToEquip;
        slot.equipedItem.AddModifiers(player.stats);
        slot.equipedItem.AddItemEffect(player);
        player.health.SetHealthToPercent(savedHealthPercent);

        RemoveOneItem(itemToEquip);
    }

    public void UnequipItem(Inventory_Item itemToUnequip, bool replacingItem = false)
    {
        if (CanAddItem(itemToUnequip) == false && replacingItem == false)
        {
            Debug.Log("No Space");
            return;
        }

        float savedHealthPercent = player.health.GetHealthPercent();

        var slotToUnequip = equipList.Find(slot => slot.equipedItem == itemToUnequip);
        if (slotToUnequip != null)
        {
            slotToUnequip.equipedItem = null;
        }
        itemToUnequip.RemoveModifiers(player.stats);
        itemToUnequip.RemoveItemEffecct();
        player.health.SetHealthToPercent(savedHealthPercent);
        AddItem(itemToUnequip);
    }

    public override void SaveData(ref GameData data)
    {
        if (gameObject.activeInHierarchy == false)
            return;

        data.gold = gold;
        data.quickItemSlot1SaveId = GetQuickItemSaveId(0);
        data.quickItemSlot2SaveId = GetQuickItemSaveId(1);
        data.inventory.Clear();
        data.equipedItems.Clear();

        foreach (var item in itemList)
        {
            if (item != null && item.itemData != null)
            {
                string saveId = item.itemData.saveId;

                if (data.inventory.ContainsKey(saveId) == false)
                    data.inventory[saveId] = 0;

                data.inventory[saveId] += item.stackSize;
            }
        }

        foreach (var slot in equipList)
        {
            if (slot.HasItem())
            {
                data.equipedItems[slot.equipedItem.itemData.saveId] = slot.slotType;

            }
        }
    }

    public override void LoadData(GameData data)
    {
        if (gameObject.activeInHierarchy == false)
            return;

        gold = data.gold;

        foreach (var entry in data.inventory)
        {
            string saveId = entry.Key;
            int stackSize = entry.Value;

            ItemDataSO itemData = itemDataBase.GetItemData(saveId);

            if (itemData == null)
            {
                Debug.LogWarning("Item not found :" + saveId);
                continue;
            }

            for (int i = 0; i < stackSize; i++)
            {
                Inventory_Item itemToLoad = new Inventory_Item(itemData);
                AddItem(itemToLoad);
            }
        }

        foreach (var entry in data.equipedItems)
        {
            string saveId = entry.Key;
            ItemType loadedSlotType = entry.Value;

            ItemDataSO itemData = itemDataBase.GetItemData(saveId);
            if (itemData == null)
            {
                Debug.LogWarning("Equipped item not found :" + saveId);
                continue;
            }

            Inventory_Item itemToLoad = new Inventory_Item(itemData);

            var slot = equipList.Find(slot => slot.slotType == loadedSlotType && slot.HasItem() == false);
            if (slot == null)
                continue;

            slot.equipedItem = itemToLoad;
            slot.equipedItem.AddModifiers(player.stats);
            slot.equipedItem.AddItemEffect(player);
        }

        LoadQuickItems(data);
        TriggerUpdateUI();
    }

    private string GetQuickItemSaveId(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickItems.Length)
            return "";

        Inventory_Item quickItem = quickItems[slotIndex];

        if (quickItem == null || quickItem.itemData == null)
            return "";

        return itemList.Exists(item => item != null && item.itemData == quickItem.itemData)
            ? quickItem.itemData.saveId
            : "";
    }

    private void LoadQuickItems(GameData data)
    {
        if (quickItems == null || quickItems.Length < 2)
            quickItems = new Inventory_Item[2];

        quickItems[0] = FindItemBySaveId(data.quickItemSlot1SaveId);
        quickItems[1] = FindItemBySaveId(data.quickItemSlot2SaveId);
    }

    private Inventory_Item FindItemBySaveId(string saveId)
    {
        if (string.IsNullOrEmpty(saveId))
            return null;

        return itemList.Find(item => item != null && item.itemData != null && item.itemData.saveId == saveId);
    }

}
