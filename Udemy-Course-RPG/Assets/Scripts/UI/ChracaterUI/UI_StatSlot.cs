using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StatSlot : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    private Player_Stats playerStats;
    private RectTransform rect;
    private UI ui;

    [SerializeField ] private StatType statSlotType;
    [SerializeField] private TextMeshProUGUI statName;
    [SerializeField] private TextMeshProUGUI statValue;

    private void OnValidate()
    {
        gameObject.name = "UI_Stat - " + GetStatNameByType(statSlotType);
        statName.text = GetStatNameByType(statSlotType);
    }
    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        playerStats = FindFirstObjectByType<Player_Stats>();
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.statToolTip.ShowToolTip(true, rect, statSlotType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.statToolTip.ShowToolTip(false,null);
    }

    public void UpdateStatValue()
    {
        Stat statToUpdate = playerStats.GetStatByType(statSlotType);
        if (statToUpdate == null && statSlotType != StatType.ElementalDamage)
        {
            Debug.Log($"没有{statSlotType}的实现在player身上");
            return;
        }
        float value = 0;
        switch (statSlotType)
        {
            // Major stats
            case StatType.Strength:
                value = playerStats.major.strength.GetValue();
                break;

            case StatType.Agility:
                value = playerStats.major.agility.GetValue();
                break;

            case StatType.Intelligence:
                value = playerStats.major.intelligence.GetValue();
                break;

            case StatType.Vitality:
                value = playerStats.major.vitality.GetValue();
                break;

            // Offense stats
            case StatType.Damage:
                value = playerStats.GetBaseDamage();
                break;

            case StatType.CritChance:
                value = playerStats.GetCritChance();
                break;

            case StatType.CritPower:
                value = playerStats.GetCritPower();
                break;

            case StatType.ArmorReduction:
                value = playerStats.GetArmorReduction() * 100;
                break;

            case StatType.AttackSpeed:
                value = playerStats.offense.attackSpeed.GetValue() * 100;
                break;
            // Defense stats
            case StatType.MaxHealth:
                value = playerStats.GetMaxHealth();
                break;

            case StatType.HealthRegen:
                value = playerStats.resources.healthRegen.GetValue();
                break;

            case StatType.Evasion:
                value = playerStats.GetEvasion();
                break;

            case StatType.Armor:
                value = playerStats.GetBaseArmor();
                break;
            // Elemental damage stats
            case StatType.IceDamage:
                value = playerStats.offense.iceDamage.GetValue();
                break;

            case StatType.FireDamage:
                value = playerStats.offense.fireDamage.GetValue();
                break;

            case StatType.LightningDamage:
                value = playerStats.offense.lightningDamage.GetValue();
                break;

            case StatType.ElementalDamage:
                value = playerStats.GetElementDamage(out ElementType element, 1);
                break;

            // Elemental resistance stats
            case StatType.IceResistance:
                value = playerStats.GetElementalResistance(ElementType.Ice)*100;
                break;

            case StatType.FireResistance:
                value = playerStats.GetElementalResistance(ElementType.Fire)*100;
                break;

            case StatType.LightningResistance:
                value = playerStats.GetElementalResistance(ElementType.Lightning)* 100;
                break;
        }

        statValue.text = IsPercentageStat(statSlotType)?value + "%":value.ToString();
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
        StatType.ElementalDamage => "Elemental Damage",
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
