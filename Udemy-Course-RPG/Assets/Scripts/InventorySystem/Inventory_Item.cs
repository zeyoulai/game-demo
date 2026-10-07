using System;
using System.Text;
using UnityEngine;

[Serializable]
public class Inventory_Item
{
    public string itemId { get; private set; }

    public ItemDataSO itemData;
    public int stackSize = 1;

    public ItemModifier[] modifiers {  get; private set; }
    public ItemEffect_DataSO itemEffect;

    public int buyPrice {  get; private set; }
    public float sellPrice {  get; private set; }

    public Inventory_Item(ItemDataSO itemData)
    {
        this.itemData = itemData;
        itemEffect = itemData.itemEffect;
        buyPrice = itemData.itemPrice;
        sellPrice = itemData.itemPrice * .35f;
        modifiers = EquitmentData()?.modifiers;

        itemId = itemData.itemName + " - "+Guid.NewGuid();
    }

    public void AddModifiers(Entity_Stats playerStats)
    {
        foreach (var modifier in modifiers)
        {
            Stat statToModify = playerStats.GetStatByType(modifier.statType);
            statToModify.AddModifier(modifier.value, itemId);
        }
    }

    public void RemoveModifiers(Entity_Stats playerStats)
    {
        foreach (var modifier in modifiers)
        {
            Stat statToModify = playerStats.GetStatByType(modifier.statType);
            statToModify.RemoveModifier(itemId);
        }
    }

    public void AddItemEffect(Player player)
    {
        itemEffect?.Subscribe(player);
    }
    public void RemoveItemEffecct() => itemEffect?.Unsubscribe();

    private EquipmentDataSO EquitmentData()
    {
        if (itemData is EquipmentDataSO equipment)
            return equipment;
        return null;
    }

    public bool CanAddStack() => stackSize < itemData.maxStackSize;
    public void AddStack()=>stackSize++;
    public void RemoveStack()=>stackSize--;

    public string GetItemInfo()
    {
        StringBuilder sb = new StringBuilder();
        if (itemData.itemType == ItemType.Material)
        {
            sb.AppendLine("");
            sb.AppendLine("Used fot crafting");
            sb.AppendLine("");
            sb.AppendLine("");
            return sb.ToString();
        }

        if (itemData.itemType == ItemType.Consumable)
        {
            sb.AppendLine("");
            sb.AppendLine(itemEffect.effectDescription);
            sb.AppendLine("");
            sb.AppendLine("");
            return sb.ToString();
        }


        sb.AppendLine("");
        foreach (var mod in modifiers)
        {
            string modType = GetStatNameByType(mod.statType);
            string modValue = IsPercentageStat(mod.statType) ? mod.value.ToString() + "%" : mod.value.ToString();
            sb.AppendLine("+ " + modValue + " " + modType);
        }

        if (itemEffect != null)
        {
            sb.AppendLine("");
            sb.AppendLine("Unique effect:");
            sb.AppendLine(itemEffect.effectDescription);
        }
        sb.AppendLine("");
        sb.AppendLine("");
        return sb.ToString();
    }

    private string GetStatNameByType(StatType type) => type switch
    {
        StatType.MaxHealth => "Max Health",
        StatType.HealthRegen => "Health Regeneration",
        StatType.Strength => "Strength",
        StatType.Agility => "Agility",
        StatType.Intelligence => "Intelligence",
        StatType.Vitality => "Vitality",
        StatType.AttackSpeed => "Attack Speed",
        StatType.Damage => "Damage",
        StatType.CritChance => "Critical Chance",
        StatType.CritPower => "Critical Power",
        StatType.ArmorReduction => "Armor Reduction",
        StatType.FireDamage => "Fire Damage",
        StatType.IceDamage => "Ice Damage",
        StatType.LightningDamage => "Lightning Damage",
        StatType.Armor => "Armor",
        StatType.Evasion => "Evasion",
        StatType.IceResistance => "Ice Resistance",
        StatType.FireResistance => "Fire Resistance",
        StatType.LightningResistance => "Lightning Resistance",
        _ => "Unknown Stat"
    };

    private bool IsPercentageStat(StatType type) => type switch
    {
        StatType.CritChance => true,
        StatType.CritPower => true,
        StatType.ArmorReduction => true,
        StatType.IceResistance => true,
        StatType.FireResistance => true,
        StatType.LightningResistance => true,
        StatType.AttackSpeed => true,
        StatType.Evasion => true,
        _ => false
    };
}
