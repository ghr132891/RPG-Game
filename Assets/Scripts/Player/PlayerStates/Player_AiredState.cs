using UnityEngine;

public class Player_AiredState : PlayerState
{
    public Player_AiredState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        if (player.wallDetected && rb.linearVelocity.y < 0)
        {
            // 判定条件：当前速度向下，且检测到墙壁
            stateMachine.ChangeState(player.wallSlideState);
            return;
        }

        if (player.moveInput.x != 0)
            player.SetVelocity(player.moveInput.x * (player.GetMoveSpeed() * player.inAirMoveMultiplier), rb.linearVelocity.y);

        if (input.Player.Attack.WasPressedThisFrame())
            stateMachine.ChangeState(player.jumpAttackState);
    }

}
