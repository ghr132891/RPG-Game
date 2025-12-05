using System;
using UnityEngine;

[Serializable]
public class AttackData 
{
    public float phyiscalDamage;
    public float elementDamage;
    public bool isCrit;
    public ElementType element;

    public ElementalEffectData effectData;


    public AttackData(Entity_Stats entity_Stats,DamageScaleData scaleData)
    {
        phyiscalDamage = entity_Stats.GetPhyisclDamage(out isCrit,scaleData.phyiscal);

        elementDamage = entity_Stats.GetElementalDamage(out element,scaleData.elemental);

        effectData = new ElementalEffectData(entity_Stats,scaleData);


    }




}
