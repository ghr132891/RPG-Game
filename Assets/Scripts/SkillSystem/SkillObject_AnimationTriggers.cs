using UnityEngine;

public class SkillObject_AnimationTriggers : MonoBehaviour
{
    private SkillObject_TimeEcho timeEcho;

    private void Awake()
    {
        timeEcho = GetComponentInParent<SkillObject_TimeEcho>();

    }

    private void AttackTrigger()
    {
        timeEcho.PerformAttacks();
    }

    private void TryTerminate( int currentAttackIndex)
    {
        if(currentAttackIndex == timeEcho.maxAttacks)
            timeEcho.HandleDeath();
    }
}
