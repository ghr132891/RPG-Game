using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] 
public class Stat 
{
    [SerializeField] private  float baseValue;
    [SerializeField] private List<StatModifier> modifiers = new List<StatModifier>();

    private float finalValue;
    private bool needToBeCalculated = true;
    

    public float GetValue() 
    {
        if (needToBeCalculated)
        {
            finalValue = GetFinalValue();
            needToBeCalculated = false;
        }
        return finalValue;

    }

    public void AddModifier(float value ,string source)
    {
        StatModifier moToAdd = new StatModifier(value,source);
        modifiers.Add(moToAdd);
        needToBeCalculated = true;
    }

    public void RemoveModifier(string source)
    {
        modifiers.RemoveAll(modifier => modifier.source == source);
        needToBeCalculated = true;
    }

    public float GetFinalValue()
    {
        float finalValue = baseValue;

        foreach(var modifier in modifiers)
        {
            finalValue = finalValue + modifier.value;
        }

        return finalValue;
    }

    public void SetBaseVaule(float vaule) => baseValue = vaule;

}

[Serializable]
public class StatModifier
{
    public float value;
    public string source;

    public StatModifier(float value,string source)
    {
        this.value = value;
        this.source = source;
    }


}
