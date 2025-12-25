
using UnityEngine;

public class Skill_TimeEcho : Skill_Base
{

    [SerializeField] private GameObject timeEchoPrefab;
    [SerializeField] private float timeEchoDuration;

    [Header("Attack Upgrades")]
    [SerializeField] private int maxAttacks = 3;
    [SerializeField] private float duplicateChance = .3f;

    [Header("Heal Wisp Upgrades")]
    [SerializeField] private float damagePercentHealed = .3f;
    [SerializeField] private float cooldownReducedInSeconds;

    public float GetPercentOfDamageHealed()
    {
        if (shouldBeWisp() == false)
            return 0;
        else return damagePercentHealed;

    }

    public float GetCooldownReduceInSeconds()
    {
        if (skillUpgradeType != SkillUpgradeType.TimeEcho_CooldownWisp)
            return 0;
        else return cooldownReducedInSeconds;

    }

    public bool CanRemoveNegativeEffects()
    { 
            return skillUpgradeType == SkillUpgradeType.TimeEcho_CleanseWisp;

    }


    public bool shouldBeWisp()
    {
        return skillUpgradeType == SkillUpgradeType.TimeEcho_HealWisp
            || skillUpgradeType == SkillUpgradeType.TimeEcho_CleanseWisp
            || skillUpgradeType == SkillUpgradeType.TimeEcho_CooldownWisp;

    }


    public float GetDuplicateChance()
    {
        if (skillUpgradeType != SkillUpgradeType.TimeEcho_ChanceToDuplicate)
            return 0;
        return duplicateChance;

    }

    public int GetMaxAttacks()
    {
        if (skillUpgradeType == SkillUpgradeType.TimeEcho_SingleAttack || skillUpgradeType == SkillUpgradeType.TimeEcho_ChanceToDuplicate)
            return 1;
        if (skillUpgradeType == SkillUpgradeType.TimeEcho_MultiAttack)
            return maxAttacks;

        return 0;

    }

    public float GetEchoDuration()
    {
        return timeEchoDuration;
    }
    public override void TryUseSkill()
    {
        if (CanUseSkill() == false)
            return;

       
        CreatTimeEcho();
    }



    public void CreatTimeEcho(Vector3? targetPosition = null)
    {
        Vector3 position = targetPosition ?? transform.position;

        GameObject timeEcho = Instantiate(timeEchoPrefab, position, Quaternion.identity);

        timeEcho.GetComponent<SkillObject_TimeEcho>().SetupEcho(this);

    }


}
