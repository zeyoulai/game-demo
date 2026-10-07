using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    [SerializeField] private List<StatModifier> modifiers = new List<StatModifier>();

    private bool needToCaculate = true;
    private float finalValue;

    public float GetValue()
    {
        if (needToCaculate)
        {
            finalValue = GetFinalValue();
            needToCaculate = false;
        }

        return finalValue;
    }

    //buff or items affecting base value
    //all calculations done here

    public void AddModifier(float value, string source)
    {
        modifiers.Add(new StatModifier(value, source));
        needToCaculate = true;
    }

    public void RemoveModifier(string source)
    {
        modifiers.RemoveAll(modifier => modifier.source == source);
        needToCaculate = true;
    }

    private float GetFinalValue()
    {
        float finalValue = baseValue;
        foreach (var modifier in modifiers)
        {
            finalValue += modifier.value;
        }
        return finalValue;
    }

    public void SetBaseValue(float value)
    {
        baseValue = value;
        needToCaculate = true;
    }
}

[Serializable]
public class StatModifier
{
    public float value;
    public string source;

    public StatModifier(float value, string source)
    {
        this.value = value;
        this.source = source;
    }
}