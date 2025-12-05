using UnityEngine;

public class EnemyState : EntityState
{
    protected Enemy enemy;
    public EnemyState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(stateMachine, animBoolName)
    {
        this.enemy = enemy;
        rb = enemy.rb;
        anim = enemy.anim;
        stats = enemy.stats;
    }

    

    public override void UpdateAnimationParameters()
    { 
        base.UpdateAnimationParameters();
        float battleAnimSpeedMultplier = enemy.battleMoveSpeed / enemy.movespeed;

        anim.SetFloat("battleAnimSpeedMultplier", battleAnimSpeedMultplier);
        anim.SetFloat("moveAnimSpeedMultplier",enemy.moveAnimSpeedMultplier);
        anim.SetFloat("xVelocity",rb.linearVelocity.x);
    }
}
