using Unity.VisualScripting;
using UnityEngine;

public class SkillObject_SwordSpin : SkillObject_Sword
{
    private int maxDistance;
    private float attackPerSecond;
    private float attackTimer;

    protected override void Update()
    {
        HandleComeback();
        HandleAttack();
        HandleStopping();

    }
    public override void SetupSword(Skill_SwordThrow swordManager, Vector2 direction)
    {
        base.SetupSword(swordManager, direction);

        anim?.SetTrigger("spin");

        maxDistance = swordManager.maxDistance;
        attackPerSecond = swordManager.attackPerSecond;

        Invoke(nameof(GetSwordBackToPlayer),swordManager.maxSpinDuration);

    }

    private void HandleStopping()
    {

        float distanceToplayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToplayer > maxDistance && rb.simulated == true)
            rb.simulated = false;

    }
    private void HandleAttack()
    {
        attackTimer -= Time.deltaTime;


        if (attackTimer < 0)
        {
            DamageEnemiesInRadius(transform, 1);
            attackTimer = 1 / attackPerSecond;

        }


    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        rb.simulated = false;

    }


}
