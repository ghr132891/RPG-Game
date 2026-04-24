using UnityEngine;

public class Enemy_ArcherBattleState : Enemy_BattleState
{
    private bool canFlip;
    private bool reachedDeadEnd;
    public Enemy_ArcherBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        reachedDeadEnd = false;
    }
    public override void Update()
    {
        stateTimer -= Time.deltaTime;
        UpdateAnimationParameters();

        if (enemy.groundDetected == false || enemy.wallDetected)
            reachedDeadEnd = true;

        if (enemy.PlayerDetected())
        {
            UpdateTargetIfNeeded();
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
            stateMachine.ChangeState(enemy.idleState);

        if (CanAttack())
        {
            if (enemy.PlayerDetected() == false && canFlip)
            {
                enemy.HandleFlip(DirectionToPlayer());
                canFlip = false;
            }

            enemy.SetVelocity(0, rb.linearVelocity.y);

            if (WithinAttackRange() && enemy.PlayerDetected())
            {
                canFlip = true;
                lastTimeAttacked = Time.time;
                stateMachine.ChangeState(enemy.attackState);
            }
        }
        else
        {
            bool shouldWalkAway = reachedDeadEnd == false && DistanceToPlayer() < (enemy.attackDistance * .8f);

            if (shouldWalkAway)
            {
                enemy.SetVelocity((enemy.GetBattleMoveSpeed() * -1) * DirectionToPlayer(), rb.linearVelocity.y);
            }
            else
            {
                enemy.SetVelocity(0, rb.linearVelocity.y);

                if (enemy.PlayerDetected() == false)
                    enemy.HandleFlip(DirectionToPlayer());
            }


        }
    }
}
