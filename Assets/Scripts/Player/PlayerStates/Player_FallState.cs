using UnityEngine;

public class Player_FallState : Player_AiredState
{
    public Player_FallState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        // 【新增】：在下落状态中，减少土狼时间的计时
        if (player.coyoteTimeCounter > 0)
        {
            player.coyoteTimeCounter -= Time.deltaTime;
        }

        // 【核心逻辑】：如果按下了跳跃，且土狼时间还没结束，允许起跳！
        if (player.input.Player.Jump.WasPressedThisFrame() && player.coyoteTimeCounter > 0)
        {
            player.coyoteTimeCounter = 0; // 消耗掉这次判定机会
            stateMachine.ChangeState(player.jumpState);
            return; // 提前退出，避免执行下方其他状态的切换
        }

        if (player.groundDetected)
            stateMachine.ChangeState(player.idleState);
        if (player.wallDetected)
            stateMachine.ChangeState(player.wallSlideState);
    }
}
   
