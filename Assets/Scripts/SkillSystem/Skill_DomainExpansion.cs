using UnityEngine;

public class Skill_DomainExpansion : Skill_Base
{
    [SerializeField] private GameObject domainPrefab;

    [Header("Slowing Dowm Upgrades")]
    [SerializeField] private float slowDownPercent = .8f;
    [SerializeField] private float slowDownDomainDuration = 5f;

    [Header("Spell Casting Upgrades")]
    [SerializeField] private float spellCastingDomainSlowDownPercent = 1;
    [SerializeField] private float spellCastingDomainDuration = 10;

    [Header("Domain Details")]
    public float maxDomainSize = 10;
    public float expandSpeed = 3;

    public float GetDomainDuration()
    {
        if(skillUpgradeType == SkillUpgradeType.Domain_SlowingDown)
            return slowDownDomainDuration;
        else
            return spellCastingDomainDuration;
    }

    public float GetSlowPercentage()
    {
        if(skillUpgradeType == SkillUpgradeType.Domain_SlowingDown)
            return slowDownPercent;
        else
            return spellCastingDomainSlowDownPercent;
    } 

    public bool InstantDomain()
    {
        return skillUpgradeType != SkillUpgradeType.Domain_EchoSpam
            && skillUpgradeType != SkillUpgradeType.Domain_ShardSpam;
    }

    public void CreateDomain()
    {
        GameObject domain = Instantiate(domainPrefab, transform.position, Quaternion.identity);
        domain.GetComponent<SkillObject_DomainExpansion>().SetUpDomain(this);

    }
}
