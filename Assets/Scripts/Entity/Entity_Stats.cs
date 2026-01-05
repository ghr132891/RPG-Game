using UnityEngine;

public class Entity_Stats : MonoBehaviour
{
    public Stat_SetupDataSo defaultStatSetup;

    public Stat_ResourceGroup resources;
    public Stat_OffenseGroup offense;
    public Stat_DefenseGroup defense;
    public Stat_MajorGroup major;



    public AttackData GetAttackData(DamageScaleData scaleData)
    {
        return new AttackData(this, scaleData);
    }

    //物理攻击
    public float GetPhyisclDamage(out bool isCrit, float scaleFactor = 1)
    {
        float baseDamage = GetBaseDamage();
        float critChance = GetCritChance();
        float critPower = GetCritPower() / 100;

        isCrit = Random.Range(0, 100) < critChance;
        float finalDamage = isCrit ? baseDamage * critPower : baseDamage;

        return finalDamage * scaleFactor;

    }
    //each strength gives you 1 damage
    public float GetBaseDamage() => offense.damage.GetValue() + major.strength.GetValue();
    //each agility gives you .3 of critchance
    public float GetCritChance() => offense.critChance.GetValue() + (major.agility.GetValue() * .3f);
    //each strength gives you .5 of critpower
    public float GetCritPower() => offense.critPower.GetValue() + (major.strength.GetValue() * .5f);


    //元素伤害：只能造成按照一定比例造成伤害(scaleFactor)
    //取三者最大值按照100%权重计算伤害，其余两种元素按50%权重计算伤害
    public float GetElementalDamage(out ElementType element, float scaleFactor = 1)
    {
        float fireDamage = offense.fireDamage.GetValue();
        float iceDamage = offense.iceDamage.GetValue();
        float lightningDamage = offense.lightningDamage.GetValue();

        float bonusElementalDamage = major.intelligence.GetValue(); // each intelligence gives you 1 elementalDamage

        float highestDamge = fireDamage;
        element = ElementType.Fire;

        if (iceDamage > highestDamge)
        {
            highestDamge = iceDamage;
            element = ElementType.Ice;
        }

        if (lightningDamage > highestDamge)
        {
            highestDamge = lightningDamage;
            element = ElementType.Lightning;
        }

        if (highestDamge <= 0)
        {
            element = ElementType.None;
            return 0;
        }

        float bonusfire = (element == ElementType.Fire) ? 0 : fireDamage * .5f;
        float bonusice = (element == ElementType.Ice) ? 0 : iceDamage * .5f;
        float bonusLightning = (element == ElementType.Lightning) ? 0 : lightningDamage * .5f;
        float weakerElementalDamage = bonusfire + bonusice + bonusLightning;

        float finalDamage = highestDamge + weakerElementalDamage + bonusElementalDamage;

        return finalDamage * scaleFactor;
    }
    //元素抗性
    public float GetElementalResistance(ElementType element)
    {
        float baseResistance = 0;
        float bonusResistance = major.intelligence.GetValue() * .5f;// each intelligence gives you .5f elementalResistance

        switch (element)
        {
            case ElementType.Fire:
                baseResistance = defense.fireRes.GetValue();
                break;
            case ElementType.Ice:
                baseResistance = defense.iceRes.GetValue();
                break;
            case ElementType.Lightning:
                baseResistance = defense.lightningRes.GetValue();
                break;
        }
        float resistance = baseResistance + bonusResistance;
        float resistanceCap = 75f;
        float finalResistance = Mathf.Clamp(resistance, 0, resistanceCap) / 100;

        return finalResistance;

    }

    //护甲减免
    public float GetArmorMitigation(float armorReduction)
    {

        float totalArmor = GetBaseArmor();

        float reductionMutliplier = Mathf.Clamp(1 - armorReduction, 0, 1);
        float effectiveArmor = totalArmor * reductionMutliplier;

        float armorMitigationCap = .85f;
        float armormitigation = effectiveArmor / (effectiveArmor + 100);

        float finalArmorMitigation = Mathf.Clamp(armormitigation, 0, armorMitigationCap);

        return finalArmorMitigation;
    }
    //each vitality gives you 1 armor
    public float GetBaseArmor() => defense.armor.GetValue() + major.vitality.GetValue();
    //护甲穿透
    public float GetArmorReduction()
    {

        float finalArmorReduction = offense.armorReduction.GetValue() / 100;
        return finalArmorReduction;
    }

    //血量
    public float GetMaxHealth()
    {
        float baseMaxHealth = resources.maxHealth.GetValue();
        float bonusMaxHealth = major.vitality.GetValue() * 5;// each vitality gives you 5 Hp
        float finalMaxHealth = baseMaxHealth + bonusMaxHealth;

        return finalMaxHealth;
    }
    //闪避
    public float GetEvasion()
    {
        float baseEvasion = defense.evasion.GetValue();
        float bonusEvasion = major.agility.GetValue() * .005f;// each agility gives you .5% of evasion 

        float totalEvasion = baseEvasion + bonusEvasion;
        float evasionCap = 85f; //you have 85% chance to evade the attack

        return Mathf.Clamp(totalEvasion, 0, evasionCap);
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
                Debug.LogWarning($"StatType {type} not implemented yet.");
                return null;




        }


    }


    [ContextMenu("Updata Default Stat Setup")]
    public void ApplyDefaultStatSetup()
    {
        if (defaultStatSetup == null)
        {
            Debug.Log("No default stat setup assigned");
            return;
        }

        resources.maxHealth.SetBaseVaule(defaultStatSetup.maxHealth);
        resources.healthRegen.SetBaseVaule(defaultStatSetup.healthRegen);

        major.strength.SetBaseVaule(defaultStatSetup.strength);
        major.agility.SetBaseVaule(defaultStatSetup.agility);
        major.intelligence.SetBaseVaule(defaultStatSetup.intelligence);
        major.vitality.SetBaseVaule(defaultStatSetup.vitality);

        offense.attackSpeed.SetBaseVaule(defaultStatSetup.attackSpeed);
        offense.damage.SetBaseVaule(defaultStatSetup.damage);
        offense.critChance.SetBaseVaule(defaultStatSetup.critChance);
        offense.critPower.SetBaseVaule(defaultStatSetup.critPower);
        offense.armorReduction.SetBaseVaule(defaultStatSetup.armorReduction);

        offense.fireDamage.SetBaseVaule(defaultStatSetup.fireDamage);
        offense.iceDamage.SetBaseVaule(defaultStatSetup.iceDamage);
        offense.lightningDamage.SetBaseVaule(defaultStatSetup.lightningDamage);

        defense.armor.SetBaseVaule(defaultStatSetup.armor);
        defense.evasion.SetBaseVaule(defaultStatSetup.evasion);

        defense.fireRes.SetBaseVaule(defaultStatSetup.fireResistance);
        defense.iceRes.SetBaseVaule(defaultStatSetup.iceResistance);
        defense.lightningRes.SetBaseVaule(defaultStatSetup.lightningResistance);


    }
}
