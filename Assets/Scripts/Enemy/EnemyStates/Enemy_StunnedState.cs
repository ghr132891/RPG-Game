using UnityEngine;

public class Enemy_StunnedState : EnemyState
{
    private Enemy_VFX enemy_VFX;

    public Enemy_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemy_VFX = enemy.GetComponent<Enemy_VFX>();

    }

    public override void Enter()
    {
        base.Enter();
        enemy_VFX.EnableAttackAlert(false);
        enemy.EnableCounterWindow(false);
        stateTimer = enemy.stunnedDuration;
        rb.linearVelocity = new Vector2(enemy.stunnedVelocity.x * -enemy.facingDir, enemy.stunnedVelocity.y);
    }

    public override void Update()
    {
        base.Update();
        if (stateTimer < 0)
            stateMachine.ChangeState(enemy.idleState);
    }
}
