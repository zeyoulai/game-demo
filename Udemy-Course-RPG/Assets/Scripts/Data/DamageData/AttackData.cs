using System;
using UnityEngine;


[Serializable]
public class AttackData
{
    public float physicalDamage;
    public float elementDamage;
    public bool isCrit;
    public ElementType element;

    public ElementalEffectData effectData;

    public AttackData(Entity_Stats entityStats,DamageScaleData scaleData)
    {
        physicalDamage = entityStats.GetPhyiscalDamage(out isCrit, scaleData.phsycal);
        elementDamage = entityStats.GetElementDamage(out element,scaleData.elemental);

        effectData = new ElementalEffectData(entityStats,scaleData);

    }
}
