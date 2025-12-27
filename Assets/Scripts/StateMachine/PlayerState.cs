using UnityEngine;

public abstract class PlayerState : EntityState
{
    protected Player player;
    protected PlayerInputSet input;
    protected Player_SkillManager skillManager;

    public PlayerState(Player player, StateMachine stateMachine, string animBoolName):base(stateMachine,animBoolName)
    {
        this.player = player;  

        anim = player.anim;
        rb = player.rb;
        input = player.input;
        stats = player.stats;
        skillManager = player.skillManager;
    }

    public override void Update()
    {
        base.Update();

        

        if (input.Player.Dash.WasPressedThisFrame() && CanDash())
        {
            skillManager.dash.SetSkillOnCooldown();

            stateMachine.ChangeState(player.dashState);
        }

        if(input.Player.UltimateSpell.WasPressedThisFrame() && skillManager.domainExpansion.CanUseSkill())
        {
            if (skillManager.domainExpansion.InstantDomain())
            {
                skillManager.domainExpansion.CreateDomain();
            }
            else
            {
                stateMachine.ChangeState(player.domainExpansionState);
            }

            skillManager.domainExpansion.SetSkillOnCooldown();

        }
    }


    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();
        anim.SetFloat("yVelocity",rb.linearVelocity.y);

    }

    private bool CanDash()
    {
        if(skillManager.dash.CanUseSkill() == false)
            return false;

        if(player.wallDetected)
            return false;
        if(stateMachine.currentState == player.dashState || stateMachine.currentState == player.domainExpansionState)
            return false;

        return true;
    }

}
