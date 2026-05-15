using UnityEngine;

public class Enemy_DeadState : EnemyState
{
    private Collider2D col;

    public Enemy_DeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        col = enemy.GetComponent<Collider2D>();
    }

    public override void Enter()
    {
        // 1. 【最核心修复】：必须调用基类 Enter，这才会执行 anim.SetBool("dead", true) 触发动画！
        base.Enter();

        TimeRewinder rewinder = enemy.GetComponent<TimeRewinder>();
        if (rewinder != null)
        {
            rewinder.SetDead();
        }

        // 2. 【最核心修复】：删除了 anim.enabled = false，让动画器保持开启状态！

        // 3. 通用死亡物理处理（停在原地、取消碰撞）
        if (col != null) col.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // 4. 关闭状态机的 Update 循环，防止死后怪物还能乱动或触发其他逻辑
        stateMachine.SwitchOffStateMachine();

        // （已删除旧的弹飞代码 rb.gravityScale = 10...）
        // （已删除旧的定时销毁代码 enemy.DestoryGameObjectWithDealy...）
        // 现在，怪物的销毁任务已经完美交给了我们之前设置的 Animation Event (SelfDestroyTrigger)
    }
}
