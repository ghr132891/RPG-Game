using UnityEngine;

public class Skill_TimeEcho : Skill_Base
{
    
    [SerializeField] private GameObject timeEchoPrefab;
    [SerializeField] private float timeEchoDuration;

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

    

    public void CreatTimeEcho()
    {
        GameObject timeEcho = Instantiate(timeEchoPrefab,transform.position,Quaternion.identity);

        timeEcho.GetComponent<SkillObject_TimeEcho>().SetupEcho(this);

    }


}
