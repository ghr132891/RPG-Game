using System.Xml.Serialization;
using UnityEngine;

public class SkillObject_Sword : SkillObject_Base
{
    protected Skill_SwordThrow swordManager;
    

    protected Transform playerTransform;
    protected bool shouldComeback;
    protected float comebackSpeed =30;

    protected float maxAllowedDistance = 25;


    protected virtual void Update()
    {
        transform.right = rb.linearVelocity;

        HandleComeback();
    }
    public virtual void SetupSword(Skill_SwordThrow swordManager,Vector2 direction)
    {
        
        rb.linearVelocity = direction;

        this.swordManager = swordManager;

        playerTransform = swordManager.transform.root;

        playerStats = swordManager.player.stats;
        damageScaleData = swordManager.damageScaleData;

    }

    public void GetSwordBackToPlayer() => shouldComeback = true;

    protected void HandleComeback()
    {
        float distacne = Vector2.Distance(transform.position, playerTransform.position);

        if(distacne > maxAllowedDistance)
        {
            GetSwordBackToPlayer();
            rb.simulated = false;

        }

        if (shouldComeback == false)
            return;

        transform.position = Vector2.MoveTowards(transform.position,playerTransform.position,comebackSpeed * Time.deltaTime);

        if (distacne < .5f)
            Destroy(gameObject);


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
