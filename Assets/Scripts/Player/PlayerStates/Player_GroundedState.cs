using UnityEngine;

public class Player_GroundedState : PlayerState
{
    public Player_GroundedState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();
        // 【新增】：只要玩家在地面状态，就一直重置土狼时间
        player.coyoteTimeCounter = player.coyoteTime;

        if (rb.linearVelocity.y < 0 && player.groundDetected == false)
            stateMachine.ChangeState(player.fallState);

        if (input.Player.Jump.WasPressedThisFrame())
        {
            // 【新增】：主动起跳时立即清空土狼时间，防止跳跃到空中后还能触发判定
            player.coyoteTimeCounter = 0;
            stateMachine.ChangeState(player.jumpState); 
        }

        if (input.Player.Attack.WasPressedThisFrame())
            stateMachine.ChangeState(player.basicAttackState);

        if (input.Player.CounterAttack.WasPressedThisFrame() && player.CanCounter())
            stateMachine.ChangeState(player.counterAttackState);

        if (input.Player.RangeAttack.WasPressedThisFrame() && skillManager.swordThrow.CanUseSkill())
            stateMachine.ChangeState(player.swordThrowState);
    }

}
