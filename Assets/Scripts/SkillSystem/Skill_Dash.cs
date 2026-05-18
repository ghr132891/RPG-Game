using UnityEngine;

public class Skill_Dash : Skill_Base
{

    public override bool CanUseSkill()
    {
        // 直接干掉，不让用
        return false;

        // 原本的代码可能是这样的，不用管它：
        // return base.CanUseSkill() && skillUnlocked;
    }

    public void OnStartEffect()
    {
        if (Unlocked(SkillUpgradeType.Dash_CloneOnStart) || Unlocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
        {
            CreatClone();
        }

        if(Unlocked(SkillUpgradeType.Dash_ShardOnStart) || Unlocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
        {
            CreatShard();
        }

    }
     
    public void OnEndEffect()
    {
        if (Unlocked(SkillUpgradeType.Dash_CloneOnStartAndArrival))
        {
            CreatClone();
        }

        if (Unlocked(SkillUpgradeType.Dash_ShardOnStartAndArrival))
        {
            CreatShard();
        }

    }


    private void CreatShard()
    {
        skillManager.shard.CreatRawShard();
    }

    private void CreatClone()
    {
        skillManager.timeEcho.CreatTimeEcho();
    }


}
