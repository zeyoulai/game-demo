
using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour,ISaveable
{
    protected Player player;
    public event Action OnInventoryChange;
    public int maxInventorySize = 10;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    [Header("ITEM DATA BASE")]
    [SerializeField] protected ItemListDataSO itemDataBase;


    protected virtual void Awake()
    {
        player = GetComponent<Player>();
    }

    public void TryUseItem(Inventory_Item itemToUse) //BUG 使用药水分不清哪个是哪个，
    {
        Inventory_Item consumable = itemList.Find(item => item == itemToUse);
        if (consumable == null) return;
        if (consumable.itemEffect == null) return;

        if (consumable.itemEffect.CanBeUsed(player) == false)
            return;

        consumable.itemEffect.ExecuteEffect();
        Debug.Log("consumable:" + consumable.stackSize);
        if (consumable.stackSize > 1)
            consumable.RemoveStack();
        else
            RemoveOneItem(consumable);

        OnInventoryChange?.Invoke();
    }



    public bool CanAddItem(Inventory_Item itemToAdd)
    {
        bool hasStackable = FindStackable(itemToAdd) != null;
        return hasStackable || itemList.Count < maxInventorySize;
    }

    public Inventory_Item FindStackable(Inventory_Item itemToAdd)
    {
        return itemList.Find(item => item.itemData == itemToAdd.itemData && item.CanAddStack());

    }
    public void AddItem(Inventory_Item itemToAdd)
    {


        Inventory_Item itemInventory = FindStackable(itemToAdd);
        if (itemInventory != null)
        {
            itemInventory.AddStack();
        }
        else
        {
            itemList.Add(itemToAdd);
        }

        OnInventoryChange?.Invoke();
    }

    public Inventory_Item FindItem(Inventory_Item itemToFind)
    {
        return itemList.Find(item => item == itemToFind);
    }

    public Inventory_Item FindSameItem(Inventory_Item itemToFind)
    {
        return itemList.Find(item => item.itemData == itemToFind.itemData);
    }

    public Inventory_Item FindItemByItemId(Inventory_Item itemToRemove)
    {
        return itemList.Find(item => item.itemId == itemToRemove.itemId);
    }

    public void RemoveOneItem(Inventory_Item itemToRemove)
    {
        if (itemToRemove == null)
            return;

        Inventory_Item itemInInventory = itemList.Find(item=>item ==  itemToRemove);

        if (itemInInventory == null)
            return;

        if (itemInInventory.stackSize > 1)
        {
            Debug.Log(itemInInventory.stackSize);
            itemInInventory.RemoveStack();
        }
        else
        {
            itemList.Remove(itemToRemove);
        }

            //itemList.Remove(FindItemByItemId(itemToRemove));
            OnInventoryChange?.Invoke();
    }

    public void RemoveFullStack(Inventory_Item itemToRemove)
    {
        for (int i = 0;i<itemToRemove.stackSize;i++)
        {
            RemoveOneItem(itemToRemove);
        }
    }

    public void RemoveItemAmount(ItemDataSO itemToRemove,int amount)
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            Inventory_Item item = itemList[i];

            if (item.itemData != itemToRemove)
                continue;

            int removeCount = Mathf.Min(amount, item.stackSize);

            for (int j = 0; j < removeCount; j++)
            {
                RemoveOneItem(item);
                amount--;

                if (amount <= 0)
                    break;
            }
        }

    }

    public bool HasItemAmount(ItemDataSO itemToCheck,int amount)
    {
        int total = 0;

        foreach (var item in itemList)
        {
            if (item.itemData == itemToCheck)
                total = total + item.stackSize;

            if (total >= amount)
                return true;
        }

        return false;
    }

    /*   public void RemoveItem(Inventory_Item itemToRemove)
       { 原本的防止药水bug的方法 不过我改成ById了
           itemList.Remove(itemToRemove);
           OnInventoryChange?.Invoke();
       }*/

    public void TriggerUpdateUI() => OnInventoryChange?.Invoke();

    public virtual void LoadData(GameData data)
    {
        
    }

    public virtual void SaveData(ref GameData data)
    {
        
    }
}
