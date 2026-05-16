using UnityEngine;

public class Enemy_ArcherBattleState : Enemy_BattleState
{
    private bool canFlip;

    // 这个变量现在只代表“正前方”是否有墙壁或悬崖
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

        // 1. 【前方路况检测】：只影响追击，不影响撤退
        reachedDeadEnd = (enemy.groundDetected == false || enemy.wallDetected);

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

            if (WithinAttackRange() && enemy.PlayerDetected())
            {
                // 玩家在射程内，停步射箭
                enemy.SetVelocity(0, rb.linearVelocity.y);
                canFlip = true;
                lastTimeAttacked = Time.time;
                stateMachine.ChangeState(enemy.attackState);
            }
            else
            {
                // 追击玩家
                float xVelocity = enemy.canChasePlayer ? enemy.GetBattleMoveSpeed() : 0f;

                if (reachedDeadEnd) // 如果前方没路了，停下脚步
                {
                    xVelocity = 0f;
                }

                // 前进时可以使用 SetVelocity，因为它会自动处理正常的面朝方向
                enemy.SetVelocity(xVelocity * DirectionToPlayer(), rb.linearVelocity.y);
            }
        }
        else
        {
            // ==========================================
            // 2. 【后方路况检测】：专门用于控制安全撤退
            // ==========================================
            float backCheckDistance = 1.5f;

            // A. 检测背后有没有墙壁
            Vector2 wallCheckOrigin = new Vector2(enemy.transform.position.x, enemy.primaryWallCheck.position.y);
            bool wallBehind = Physics2D.Raycast(wallCheckOrigin, Vector2.right * -enemy.facingDir, backCheckDistance, enemy.whatIsGround);

            // B. 检测背后有没有悬崖 (从背后向下打一条射线)
            Vector2 ledgeCheckOrigin = new Vector2(enemy.transform.position.x + (-enemy.facingDir * backCheckDistance), enemy.groundCheck.position.y);
            bool ledgeBehind = !Physics2D.Raycast(ledgeCheckOrigin, Vector2.down, 2.0f, enemy.whatIsGround);

            // 在 Scene 窗口画出辅助线，方便你直观看到背后的探测范围
            Debug.DrawRay(wallCheckOrigin, Vector2.right * -enemy.facingDir * backCheckDistance, wallBehind ? Color.green : Color.red);
            Debug.DrawRay(ledgeCheckOrigin, Vector2.down * 2.0f, ledgeBehind ? Color.yellow : Color.blue);

            // 如果背后有墙 或 背后有悬崖，就禁止撤退！
            bool cantRetreat = wallBehind || ledgeBehind;

            // 撤退条件：玩家太近 且 背后安全 (注意：这里不再受 reachedDeadEnd 影响！)
            bool shouldWalkAway = DistanceToPlayer() < (enemy.attackDistance * .8f) && !cantRetreat;

            if (shouldWalkAway)
            {
                // 【核心修复】：绝不调用 SetVelocity！直接修改刚体速度！
                // 这样就能实现在不转身的情况下，“倒退走”风筝玩家，彻底杜绝左右抽搐！
                float retreatVelocity = (enemy.GetBattleMoveSpeed() * -1) * DirectionToPlayer();
                rb.linearVelocity = new Vector2(retreatVelocity, rb.linearVelocity.y);

                // 强制将脸转过来，死死盯着玩家
                if (DirectionToPlayer() == 1 && enemy.facingDir != 1) enemy.Flip();
                else if (DirectionToPlayer() == -1 && enemy.facingDir != -1) enemy.Flip();
            }
            else
            {
                // 技能冷却中，且背后没路了/距离安全，就在原地罚站
                enemy.SetVelocity(0, rb.linearVelocity.y);

                if (enemy.PlayerDetected() == false)
                    enemy.HandleFlip(DirectionToPlayer());
            }
        }
    }
}