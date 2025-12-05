using UnityEngine;

public class Player_SwordThrowState : PlayerState
{

    private Camera maincamera;
    public Player_SwordThrowState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }


    public override void Enter()
    {
        base.Enter();

        if (maincamera != Camera.main)
            maincamera = Camera.main;

        skillManager.swordThrow.EnableDots(true);
    }
    public override void Update()
    {
        base.Update();

        Vector2 dirToMouse = DirectionToMouse();


        player.SetVelocity(0, rb.linearVelocity.y);
        player.HandleFlip(dirToMouse.x);
        skillManager.swordThrow.PredictTrajectory(dirToMouse);

        if (input.Player.Attack.WasPressedThisFrame())
        {
            anim.SetBool("swordThrowPerformed", true);

            skillManager.swordThrow.EnableDots(false);
            skillManager.swordThrow.ConfirmTrajectory(dirToMouse);

        }


        if (input.Player.RangeAttack.WasReleasedThisFrame() || triggerCalled)
            stateMachine.ChangeState(player.idleState);




    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("swordThrowPerformed", false);
        skillManager.swordThrow.EnableDots(false);

    }

    private Vector2 DirectionToMouse()
    {
        Vector2 playerPosition = player.transform.position;

        Vector2 worldMousePosition = maincamera.ScreenToWorldPoint(player.mousePosition);

        Vector2 direction = worldMousePosition - playerPosition;

        return direction.normalized;


    }
}
