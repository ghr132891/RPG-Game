using UnityEngine;

public class Skill_WorldSwitch : Skill_Base
{
    [Header("World Switch Settings")]
    public WorldType targetWorld; // 在 Inspector 中指定这个技能切向哪个世界
    public Skill_DataSO initialSkillData;

    public override void TryUseSkill()
    {
        base.TryUseSkill();

        // 调用基类的 CanUseSkill() 来检查是否解锁以及是否在冷却中
        if (CanUseSkill())
        {
            // 直接调用 WorldManager 的核心切换逻辑
            WorldManager.Instance.SwitchWorld(targetWorld);

            // 触发技能冷却并更新 UI
            SetSkillOnCooldown();
        }
    }
}
