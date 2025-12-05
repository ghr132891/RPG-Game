using UnityEngine;

public class Skill_Dash : Skill_Base
{
   

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
        Debug.Log("Creat a clone.");
    }


}
