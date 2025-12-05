using System.Xml.Serialization;
using UnityEngine;

public class SkillObject_Sword : SkillObject_Base
{
    protected Skill_SwordThrow swordManager;
    protected Rigidbody2D rb;


    private void Update()
    {
        transform.right = rb.linearVelocity;
    }
    public virtual void SetupSword(Skill_SwordThrow swordManager,Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction;

        this.swordManager = swordManager;

        playerStats = swordManager.player.stats;
        damageScaleData = swordManager.damageScaleData;

    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        StopSword(collision);

        DamageEnemiesInRadius(transform,1);


    }

    protected void StopSword(Collider2D collision)
    {
        rb.simulated = false;

        transform.parent = collision.transform;

    }

}
