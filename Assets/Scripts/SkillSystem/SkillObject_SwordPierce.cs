using Unity.VisualScripting;
using UnityEngine;

public class SkillObject_SwordPierce :SkillObject_Sword
{
    private int amountToPierce;

    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        base.SetupSword(swordManager, direction);

        amountToPierce = swordManager.amountToPierce;
    }


    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        bool groundHit = collision.gameObject.layer == LayerMask.NameToLayer("Ground");

        if (amountToPierce <= 0 || groundHit)
        {
            DamageEnemiesInRadius(transform,.3f);
            StopSword(collision);
            return;
        }

        amountToPierce--;
        DamageEnemiesInRadius(transform,.3f);

    }

}
