using UnityEngine;

public class Player_SkillManager : MonoBehaviour
{
    public Skill_Dash dash { get; private set; }
    public Skill_Shard shard { get; private set; }
    public Skill_SwordThrow swordThrow { get; private set; }
    public Skill_TimeEcho timeEcho { get; private set; }
    public Skill_DomainExpansion domainExpansion { get; private set; }
    public Skill_WorldSwitch normalSwitch { get; private set; }
    public Skill_WorldSwitch mirrorSwitch { get; private set; }
    public Skill_WorldSwitch timeSwitch { get; private set; }



    public Skill_Base[] allSkills { get; private set; }

    private void Awake()
    {
        dash = GetComponentInChildren<Skill_Dash>();
        shard = GetComponentInChildren<Skill_Shard>();
        swordThrow = GetComponentInChildren<Skill_SwordThrow>();
        timeEcho = GetComponentInChildren<Skill_TimeEcho>();
        domainExpansion = GetComponentInChildren<Skill_DomainExpansion>();
        Skill_WorldSwitch[] worldSwitches = GetComponentsInChildren<Skill_WorldSwitch>();
        foreach (var ws in worldSwitches)
        {
            if (ws.targetWorld == WorldType.Normal) normalSwitch = ws;
            if (ws.targetWorld == WorldType.Mirror) mirrorSwitch = ws;
            if (ws.targetWorld == WorldType.Time) timeSwitch = ws;
        }


        allSkills = GetComponentsInChildren<Skill_Base>();
    }

    public void ReduceAllSKillCooldownBy(float amount)
    {
        foreach (var skill in allSkills)
            skill.ReduceCooldownBy(amount);

    }

    public Skill_Base GetSkillByType(SkillType type)
    {
        switch (type)
        {
            case SkillType.Dash: return dash;
            case SkillType.TimeShard: return shard;
            case SkillType.SwordThrow: return swordThrow;
            case SkillType.TimeEcho: return timeEcho;
            case SkillType.DomainExpansion:return domainExpansion;
            case SkillType.NormalSwitch: return normalSwitch;
            case SkillType.MirrorSwitch: return mirrorSwitch;
            case SkillType.TimeSwitch: return timeSwitch;

            default:
                Debug.Log($"Skill Type{type} is not implemented.");
                return null;
        }
    }
}
