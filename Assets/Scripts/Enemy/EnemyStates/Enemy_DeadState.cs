using UnityEngine;
using UnityEngine.XR;

public class Enemy_DeadState : EnemyState
{
    private Collider2D col;
    public Enemy_DeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        col = enemy.GetComponent<Collider2D>();
    }

    public override void Enter()
    {
        anim.enabled = false;
        col.enabled = false;

        rb.gravityScale = 10;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x,20);

        stateMachine.SwitchOffStateMachine();

        
    }

}
