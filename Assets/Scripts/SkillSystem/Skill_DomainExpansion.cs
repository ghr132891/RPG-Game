using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Skill_DomainExpansion : Skill_Base
{
    [SerializeField] private GameObject domainPrefab;

    [Header("Slowing Dowm Upgrades")]
    [SerializeField] private float slowDownPercent = .8f;
    [SerializeField] private float slowDownDomainDuration = 5f;

    [Header("Shard Cast Upgrades")]
    [SerializeField] private int shardToCast = 10;
    [SerializeField] private float shardCastSlowDownPercent = 1;
    [SerializeField] private float shardCastDomainDuration = 8;
    private float spellCastTimer;
    private float spellPerSecond;

    [Header("Time Echo Cast Upgrades")]
    [SerializeField] private int echoToCast = 8;
    [SerializeField] private float echoCastSlowDownPercent = 1;
    [SerializeField] private float echoCastDomainDuration = 6;
    [SerializeField] private float healthToRestoreWithEcho = .05f;

    [Header("Domain Details")]
    public float maxDomainSize = 10;
    public float expandSpeed = 3;

    private List<Enemy> trappedTargets = new List<Enemy>();
    private Transform currentTarget;

    public void CreateDomain()
    {
        spellPerSecond = GetSpellToCast() / GetDomainDuration();

        GameObject domain = Instantiate(domainPrefab, transform.position, Quaternion.identity);
        domain.GetComponent<SkillObject_DomainExpansion>().SetUpDomain(this);

    }
    public void DoSpellCasting()
    {
        spellCastTimer -= Time.deltaTime;

        if (currentTarget == null)
            currentTarget = FindTargetInDomain();

        if (currentTarget != null && spellCastTimer < 0)
        {
            CastSpell(currentTarget);
            spellCastTimer = 1 / spellPerSecond;
            currentTarget = null;

        }
    }
    private void CastSpell(Transform target)
    {
        if (skillUpgradeType == SkillUpgradeType.Domain_EchoSpam)
        {
            Vector3 offset = Random.value < .5f ? new Vector3(1, 0) : new Vector3(-1, 0);
            skillManager.timeEcho.CreatTimeEcho(target.position + offset);
        }

        if (skillUpgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            skillManager.shard.CreatRawShard(target, true);
        }

    }

    private Transform FindTargetInDomain()
    {
        trappedTargets.RemoveAll(target => target == null || target.health.isDead);

        if(trappedTargets.Count == 0)
            return null;

        int randomIndex = Random.Range(0, trappedTargets.Count);
        return trappedTargets[randomIndex].transform;    
    }

    public float GetDomainDuration()
    {
        if (skillUpgradeType == SkillUpgradeType.Domain_SlowingDown)
            return slowDownDomainDuration;
        else if (skillUpgradeType == SkillUpgradeType.Domain_ShardSpam)
            return shardCastDomainDuration;
        else if (skillUpgradeType == SkillUpgradeType.Domain_EchoSpam)
            return echoCastDomainDuration;

        return 0;
    }

    public float GetSlowPercentage()
    {
        if (skillUpgradeType == SkillUpgradeType.Domain_SlowingDown)
            return slowDownPercent;
        else if (skillUpgradeType == SkillUpgradeType.Domain_ShardSpam)
            return shardCastSlowDownPercent;
        else if (skillUpgradeType == SkillUpgradeType.Domain_EchoSpam)
            return echoCastSlowDownPercent;

        return 0;
    }

    private int GetSpellToCast()
    {
        if (skillUpgradeType == SkillUpgradeType.Domain_ShardSpam)
            return shardToCast;
        else if (skillUpgradeType == SkillUpgradeType.Domain_EchoSpam)
            return echoToCast;

        return 0;
    }

    public bool InstantDomain()
    {
        return skillUpgradeType != SkillUpgradeType.Domain_EchoSpam
            && skillUpgradeType != SkillUpgradeType.Domain_ShardSpam;
    }



    public void AddTarget(Enemy targetToAdd)
    {
        trappedTargets.Add(targetToAdd);
    }

    public void ClearTarget()
    {
        foreach (var enemy in trappedTargets)
            enemy.StopSlowDown();

        trappedTargets = new List<Enemy>();
    }
}
