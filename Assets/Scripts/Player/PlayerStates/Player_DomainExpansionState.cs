using System;
using UnityEditor;
using UnityEngine;

public class Player_DomainExpansionState : PlayerState
{
    private Vector2 originalPosition;
    private float originalGravity;
    private float maxDistanceToGoUp;

    private bool islevitating;
    private bool createdDomain;

    public Player_DomainExpansionState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        originalPosition = player.transform.position;
        originalGravity = rb.gravityScale;
        maxDistanceToGoUp = GetAvalibleRiseDistance();

        player.SetVelocity(0,player.riseSpeed);
        player.health.SetCanTakeDamage(false);
    }

    public override void Update()
    {
        base.Update();

        if (Vector2.Distance(originalPosition, player.transform.position) >= maxDistanceToGoUp && islevitating == false)
            Levitate();

        if (islevitating)
        {
            skillManager.domainExpansion.DoSpellCasting();

            if (stateTimer < 0)
            {
                rb.gravityScale = originalGravity;
                islevitating = false;
                stateMachine.ChangeState(player.idleState);
            }
        }

    }

    public override void Exit()
    {
        base.Exit();
        
        createdDomain = false;
        player.health.SetCanTakeDamage(true);
    }

    private void Levitate()
    {
        islevitating = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;

        stateTimer = skillManager.domainExpansion.GetDomainDuration();

        if(createdDomain == false)
        {
            createdDomain = true;
            skillManager.domainExpansion.CreateDomain();
        }
    }

    private float GetAvalibleRiseDistance()
    {
        RaycastHit2D hit =
            Physics2D.Raycast(player.transform.position,Vector2.up,player.riseMaxDistance,player.whatIsGround);

        return hit.collider != null ? hit.distance - 1 : player.riseMaxDistance;
    }


}
