using UnityEngine;

public class Player_JumpAttackState : PlayerState
{

    private bool touchedGround;
    private Player_Combat combat;
    public Player_JumpAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        combat = player.GetComponent<Player_Combat>();
    }

    public override void Enter()
    {
        base.Enter();
        touchedGround = false;

        player.SetVelocity(player.GetJumpAttackVelocity().x * player.facingDir,player.GetJumpAttackVelocity().y);
    }
    public override void Update()
    {
        base.Update();

        // 只要还没触发停止逻辑
        if (touchedGround == false)
        {
            bool hitEnemy = false;

            // 1. 简单的脚底扫描敌人
            Collider2D[] colliders = Physics2D.OverlapCircleAll(player.combat.plungeCheck.position, player.combat.plungeCheckRadius);
            foreach (var hit in colliders)
            {
                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null)
                {
                    player.combat.PerformAttackOnTarget(enemy.transform); // 造成伤害
                    hitEnemy = true; // 标记打中敌人了
                    break; // 打中一个就足够了，跳出循环
                }
            }

            // 2. 【核心简化】：碰到了地面，【或者】打到了敌人，都执行停止和收招！
            if (player.groundDetected || hitEnemy)
            {
                touchedGround = true;
                anim.SetTrigger("jumpAttackTrigger");
                player.SetVelocity(0, rb.linearVelocity.y); // X轴停住，顺着重力掉落地面
            }
        }

        // 完全保留你原有的退出逻辑
        if (triggerCalled && player.groundDetected)
            stateMachine.ChangeState(player.idleState);

    }


}
