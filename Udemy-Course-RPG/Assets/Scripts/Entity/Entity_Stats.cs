using UnityEngine;

public class Entity_Stats : MonoBehaviour
{

    public Stat_SetupSO defaultStatSetup;

    public Stat_ResourcesGroup resources;
    public Stat_OffenseGroup offense;
    public Stat_DefenseGroup defense;
    public Stat_MajorGroup major;

    protected virtual void Awake()
    {

    }

    public void AdjustStatSetup(Stat_ResourcesGroup resourceGroup, Stat_OffenseGroup offenseGroup, Stat_DefenseGroup defenseGroup, float penalty, float increase)
    {
        // INCREASE STATS
        offense.damage.SetBaseValue(offenseGroup.damage.GetValue() * increase);
        offense.attackSpeed.SetBaseValue(offenseGroup.attackSpeed.GetValue() * increase);
        offense.critChance.SetBaseValue(offenseGroup.critChance.GetValue() * increase);
        offense.critPower.SetBaseValue(offenseGroup.critPower.GetValue() * increase);
        offense.fireDamage.SetBaseValue(offenseGroup.fireDamage.GetValue() * increase);
        offense.iceDamage.SetBaseValue(offenseGroup.iceDamage.GetValue() * increase);
        offense.lightningDamage.SetBaseValue(offenseGroup.lightningDamage.GetValue() * increase);

        defense.evasion.SetBaseValue(defenseGroup.evasion.GetValue() * increase);

        // PENALTY STATS
        resources.maxHealth.SetBaseValue(resourceGroup.maxHealth.GetValue() * penalty);
        resources.healthRegen.SetBaseValue(resourceGroup.healthRegen.GetValue() * penalty);

        defense.armor.SetBaseValue(defenseGroup.armor.GetValue() * penalty);
        defense.lightningRes.SetBaseValue(defenseGroup.lightningRes.GetValue() * penalty);
        defense.fireRes.SetBaseValue(defenseGroup.fireRes.GetValue() * penalty);
        defense.iceRes.SetBaseValue(defenseGroup.iceRes.GetValue() * penalty);
    }

    public AttackData GetAttackData(DamageScaleData scaleData)
    {
        return new AttackData(this,scaleData);
    }

    public float GetElementDamage(out ElementType element, float scaleFactor = 1f)
    {
        //Debug.Log(this);
        float fireDamage = offense.fireDamage.GetValue();
        float iceDamage = offense.iceDamage.GetValue();
        float lightningDamage = offense.lightningDamage.GetValue();
        float bonusElementDamage = major.intelligence.GetValue();

        float highestDamage = fireDamage;
        element = ElementType.Fire;

        if (iceDamage > highestDamage)
        {
            highestDamage = iceDamage;
            element = ElementType.Ice;
        }


        if (lightningDamage > highestDamage)
        {
            highestDamage = lightningDamage;
            element = ElementType.Lightning;
        }

        if (highestDamage <= 0)
        {
            element = ElementType.None;
            return 0;
        }

        float bonusFire = (element == ElementType.Fire) ? 0 : fireDamage * 0.5f;
        float bonusIce = (element == ElementType.Ice) ? 0 : iceDamage * .5f;
        float bonusLightning = (element == ElementType.Lightning) ? 0 : lightningDamage * 0.5f;

        float weakerElementDamage = bonusFire + bonusIce + bonusLightning;
        float finalDamage = highestDamage + weakerElementDamage + bonusElementDamage;
        return finalDamage * scaleFactor;
    }

    public float GetElementalResistance(ElementType element)
    {
        float baseResistance = 0;
        float bonusResistance = major.intelligence.GetValue() * .5f;

        if (element == ElementType.Fire)
        {
            baseResistance = defense.fireRes.GetValue();
        }
        else if (element == ElementType.Ice)
        {
            baseResistance = defense.iceRes.GetValue();
        }
        else if (element == ElementType.Lightning)
        {
            baseResistance = defense.lightningRes.GetValue();
        }

        float resistance = baseResistance + bonusResistance;
        float resistanceCap = 75f;
        float finalResistance = Mathf.Clamp(resistance, 0, resistanceCap) / 100;
        return finalResistance;


    }

    public float GetPhyiscalDamage(out bool isCrit, float scaleFactor = 1f)
    {
        float baseDamage = GetBaseDamage();

        float critChace = GetCritChance();

        float critPower = GetCritPower() / 100;

        isCrit = Random.Range(0, 100) < critChace;

        float finalDamage = isCrit ? baseDamage * critPower : baseDamage;
        return finalDamage * scaleFactor;
    }

    public float GetBaseDamage() => offense.damage.GetValue() + major.strength.GetValue();
    public float GetCritChance() => offense.critChance.GetValue() + (major.agility.GetValue() * 0.3f);
    public float GetCritPower() => offense.critPower.GetValue()+ major.strength.GetValue()* 0.5f;

    public float GetArmorMitigation(float armorReduction)
    {
        //bonus armor from vitality +1 per vit
        float totalArmor = GetBaseArmor();

        float reductionMutliplier = Mathf.Clamp(1 - armorReduction, 0, 1);
        float effectiveArmor = totalArmor * reductionMutliplier;


        float mitigation = effectiveArmor / (effectiveArmor + 100);

        float mitigationCap = 0.85f;
        float finalMgitigation = Mathf.Clamp(mitigation, 0, mitigationCap);
        return finalMgitigation;
    }

    public float GetBaseArmor()=>defense.armor.GetValue() + major.vitality.GetValue();


    public float GetArmorReduction()
    {
        float finalReduction = offense.armorReduction.GetValue() / 100;
        return finalReduction;
    }

    public float GetEvasion()
    {
        float baseEvasion = defense.evasion.GetValue();
        float bonusEvasion = major.agility.GetValue() * .5f;

        float totalEvasion = baseEvasion + bonusEvasion;
        float evasionMax = 70f;//闪避最大概率

        float finalEvasion = Mathf.Clamp(totalEvasion, 0, evasionMax);
        return finalEvasion;
    }

    public float GetMaxHealth()
    {
        float baseHp = resources.maxHealth.GetValue();
        float bonusHp = major.vitality.GetValue() * 5;
        float finalMaxHealth = baseHp + bonusHp;
        return finalMaxHealth;
    }

    public Stat GetStatByType(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHealth: return resources.maxHealth;
            case StatType.HealthRegen: return resources.healthRegen;

            case StatType.Strength: return major.strength;
            case StatType.Agility: return major.agility;
            case StatType.Intelligence: return major.intelligence;
            case StatType.Vitality: return major.vitality;

            case StatType.AttackSpeed: return offense.attackSpeed;
            case StatType.Damage: return offense.damage;
            case StatType.CritChance: return offense.critChance;
            case StatType.CritPower: return offense.critPower;
            case StatType.ArmorReduction: return offense.armorReduction;

            case StatType.FireDamage: return offense.fireDamage;
            case StatType.IceDamage: return offense.iceDamage;
            case StatType.LightningDamage: return offense.lightningDamage;

            case StatType.Armor: return defense.armor;
            case StatType.Evasion: return defense.evasion;
            case StatType.IceResistance: return defense.iceRes;
            case StatType.FireResistance: return defense.fireRes;
            case StatType.LightningResistance: return defense.lightningRes;
            default:
                Debug.LogWarning($"StatType {type} not implemented yet!");
                return null;
        }
    }

    [ContextMenu("Update Default Stat Setup")]
    public void ApplyDefaultStatSetup()
    {
        if(defaultStatSetup == null)
        {
            Debug.Log("没有默认设置可分配");
            return;
        }
        resources.maxHealth.SetBaseValue(defaultStatSetup.maxHealth);
        resources.healthRegen.SetBaseValue(defaultStatSetup.healthRegen);

        major.strength.SetBaseValue(defaultStatSetup.strength);
        major.agility.SetBaseValue(defaultStatSetup.agility);
        major.intelligence.SetBaseValue(defaultStatSetup.intelligence);
        major.vitality.SetBaseValue(defaultStatSetup.vitality);

        offense.attackSpeed.SetBaseValue(defaultStatSetup.attackSpeed);
        offense.damage.SetBaseValue(defaultStatSetup.damage);
        offense.critChance.SetBaseValue(defaultStatSetup.critChance);
        offense.critPower.SetBaseValue(defaultStatSetup.critPower);
        offense.armorReduction.SetBaseValue(defaultStatSetup.armorReduction);

        offense.fireDamage.SetBaseValue(defaultStatSetup.fireDamage);
        offense.iceDamage.SetBaseValue(defaultStatSetup.iceDamage);
        offense.lightningDamage.SetBaseValue(defaultStatSetup.lightningDamage);

        defense.armor.SetBaseValue(defaultStatSetup.armor);
        defense.evasion.SetBaseValue(defaultStatSetup.evasion);

        defense.fireRes.SetBaseValue(defaultStatSetup.fireResistance);
        defense.iceRes.SetBaseValue(defaultStatSetup.iceResistance);
        defense.lightningRes.SetBaseValue(defaultStatSetup.lightningResistance);
    }
}
