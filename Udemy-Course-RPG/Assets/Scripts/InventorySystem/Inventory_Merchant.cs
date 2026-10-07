
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Merchant : Inventory_Base
{
    private Inventory_Player inventory;

    [SerializeField] private ItemListDataSO shopData;
    [SerializeField] private int minItemAmount = 4;



    protected override void Awake()
    {
        base.Awake();
        FillShopList();
    }

    public void TryBuyItem(Inventory_Item itemToBuy,bool buyFullStack)
    {
        if (inventory == null || itemToBuy == null || itemToBuy.itemData == null)
            return;

        int amountToBuy = buyFullStack ? itemToBuy.stackSize : 1;

        for (int i = 0; i < amountToBuy; i++)
        {
            if (itemList.Contains(itemToBuy) == false || itemToBuy.stackSize <= 0)
                break;

            if (inventory.gold < itemToBuy.buyPrice)
            {
                Debug.Log("NO enough money");
                break;
            }

            if (itemToBuy.itemData.itemType == ItemType.Material)
            {
                if (inventory.storage == null)
                    break;

                inventory.storage.AddMaterialToStash(new Inventory_Item(itemToBuy.itemData));
            }
            else
            {
                if (inventory.CanAddItem(itemToBuy) == false)
                    break;

                var itemToAdd = new Inventory_Item(itemToBuy.itemData);
                inventory.AddItem(itemToAdd);
            }

            inventory.gold = inventory.gold - itemToBuy.buyPrice;
            RemoveOneItem(itemToBuy);
        }

        inventory.TriggerUpdateUI();
        TriggerUpdateUI();
    }

    public void TrySellItem(Inventory_Item itemToSell, bool sellFullStack)
    {
        if (inventory == null || itemToSell == null || itemToSell.itemData == null)
            return;

        int amountToSell = sellFullStack ? itemToSell.stackSize : 1;

        for (int i = 0; i < amountToSell; i++)
        {
            if (inventory.itemList.Contains(itemToSell) == false || itemToSell.stackSize <= 0)
                break;

            int sellPrice = Mathf.FloorToInt(itemToSell.sellPrice);

            inventory.gold = inventory.gold + sellPrice;
            inventory.RemoveOneItem(itemToSell);
        }

        inventory.TriggerUpdateUI();
        TriggerUpdateUI();
    }

    public void FillShopList()
    {
        itemList.Clear();
        List<Inventory_Item> possibleItems = new List<Inventory_Item>();
        foreach (var itemData in shopData.itemList)
        {
            int randmoziedStack = Random.Range(itemData.minStackSizeAtShop, itemData.maxStackSizeAtShop+1);
            int finalStack = Mathf.Clamp(randmoziedStack, 1, itemData.maxStackSize);

            Inventory_Item itemToAdd = new Inventory_Item(itemData);
            itemToAdd.stackSize = finalStack;

            possibleItems.Add(itemToAdd);

        }

        int randomItemAmount = Random.Range(minItemAmount, maxInventorySize+1);
        int fianlAmount = Mathf.Clamp(randomItemAmount, 1, possibleItems.Count);

        for(int i = 0; i < fianlAmount; i++)
        {
            var randomIndex = Random.Range(0,possibleItems.Count);
            var item = possibleItems[randomIndex];

            if (CanAddItem(item))
            {
                possibleItems.Remove(item);
                AddItem(item);
            }
        }

        TriggerUpdateUI();
    }

    public void SetInventory(Inventory_Player inventory) => this.inventory = inventory;
}
