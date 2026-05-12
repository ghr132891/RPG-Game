using UnityEngine;

public class Enemy_StunnedState : EnemyState
{
    private Enemy_VFX enemy_VFX;

    public Enemy_StunnedState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemy_VFX = enemy.GetComponent<Enemy_VFX>();
    }

    public override void Enter()
    {
        base.Enter();
        enemy_VFX.EnableAttackAlert(false);
        enemy.EnableCounterWindow(false);
        stateTimer = enemy.stunnedDuration;

        // 为了新弹反赋予的击退力
        rb.linearVelocity = new Vector2(enemy.stunnedVelocity.x * -enemy.facingDir, enemy.stunnedVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // 当眩晕硬直时间结束时
        if (stateTimer < 0)
        {
            // 1. 检查 Animator 中是否配置了需要播放“恢复动画”（如法师）
            if (enemy.anim.GetBool("hasStunRecovery"))
            {
                // 手动把 stunned 设为 false，触发 Animator 从 Stunned 过渡到 StunnedRecovery
                enemy.anim.SetBool(animBoolName, false);

                // 等待恢复动画播放完毕（通过动画帧事件触发 triggerCalled 为 true）
                if (triggerCalled)
                {
                    enemy.OnStunFinished();
                }
            }
            else
            {
                // 2. 如果是没有恢复动画的怪物（比如史莱姆），时间一到直接进入 Idle
                enemy.OnStunFinished();
            }
        }
    }
}
