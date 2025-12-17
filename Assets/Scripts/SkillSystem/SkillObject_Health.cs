using UnityEditor;
using UnityEngine;

public class SkillObject_Health : Entity_Health
{

    protected override void Die()
    {
        SkillObject_TimeEcho timeEcho = GetComponent<SkillObject_TimeEcho>();
        timeEcho.HandleDeath();
    }

}
