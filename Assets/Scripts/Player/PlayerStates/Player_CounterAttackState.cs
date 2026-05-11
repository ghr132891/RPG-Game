using UnityEngine;

public class Player_CounterAttackState : PlayerState
{
    private Player_Combat combat;
    private bool counterSomeone;

    public Player_CounterAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        combat = player.GetComponent<Player_Combat>();
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = combat.GetCounterRecoveryDuration();
        counterSomeone = combat.CounterAttackPerformed();

        anim.SetBool("counterAttackPerformed", counterSomeone);
    }

    public override void Update()
    {
        base.Update();

        // 【修改核心】：只有在地面上弹反时，才锁死 X 轴速度防止滑步。
        // 如果是在空中（下落跑酷），绝对不锁死 X 轴，以保留弹反获得的反推力！
        if (player.groundDetected)
        {
            player.SetVelocity(0, rb.linearVelocity.y);
        }

        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);

        if (stateTimer < 0 && counterSomeone == false)
            stateMachine.ChangeState(player.idleState);
    }
}



