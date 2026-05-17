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
        base.Enter();

        TimeRewinder rewinder = enemy.GetComponent<TimeRewinder>();
        if (rewinder != null)
        {
            rewinder.SetDead();
        }

        // ==========================================
        // 【核心修改区：尸体物理保留逻辑】
        // ==========================================

        // 1. 绝对不能关闭碰撞体！让它继续触发压力板
        // if (col != null) col.enabled = false; // （已注释掉）

        // 2. 保持刚体为 Dynamic（动态），让尸体受重力自然掉落到压力板上
        rb.bodyType = RigidbodyType2D.Dynamic;

        // 3. 将 X 轴速度清零防止尸体在地上无限滑行，但保留 Y 轴下落速度
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // 4. 【关键体验优化】：切换到“尸体层”
        // 这样尸体能压机关，但不会变成一堵墙挡住玩家的路
        int corpseLayer = LayerMask.NameToLayer("Corpse");
        if (corpseLayer != -1) // 确保你在 Unity 里建了这个层
        {
            enemy.gameObject.layer = corpseLayer;
        }

        // ==========================================

        // 关闭状态机的 Update 循环
        stateMachine.SwitchOffStateMachine();
    }
}