using UnityEngine;

public class Player_Combat : Entity_Combat
{
    [Header("Counter Attack Details")]
    [SerializeField] private float counterRadius = 2.5f; // 光圈弹反的范围半径
    [SerializeField] private float knockbackForce = 15f; // 对敌人造成的击退力度
    
    [Header("空中跑酷物理参数")]
    [SerializeField] private float selfPushForce = 12f;  // 弹反成功后，玩家自身的反冲力大小
    [SerializeField] private float minUpwardBoost = 3f;  // 无论往哪边反冲，都给一点向上的力，提升手感

    [SerializeField] private float counterRecovery = .1f;
    [SerializeField] private LayerMask whatisCounterable;

    public bool CounterAttackPerformed()
    {
        bool hasCounteredSomeone = false;
        Vector2 finalRecoilDir = Vector2.zero; // 记录玩家的反冲方向

        // 使用圆形范围检测代替原来的前方检测，实现“以自身为中心的光圈”
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, counterRadius, whatisCounterable);

        foreach (var target in colliders)
        {
            ICounterable counterable = target.GetComponent<ICounterable>();

            if (counterable == null)
                continue;

            if (counterable.CanBeCountered)
            {
                counterable.HandleCounter();
                hasCounteredSomeone = true;

                // 1. 计算击退怪物的方向（玩家中心 -> 怪物中心）
                Vector2 knockbackDir = (target.transform.position - transform.position).normalized;
                
                // 给怪物施加真实的物理击退
                Rigidbody2D enemyRb = target.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.linearVelocity = Vector2.zero; // 先清空怪物速度
                    enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                }

                // 2. 记录玩家的反冲方向（即击退怪物的反方向）
                finalRecoilDir = -knockbackDir;
            }
        }

        // 3. 施加玩家自身位移（跑酷核心）
        if (hasCounteredSomeone)
        {
            Player player = GetComponent<Player>();
            Rigidbody2D playerRb = GetComponent<Rigidbody2D>();

            // 只有在空中（脚下没有检测到地面）时，才给自己施加反冲力
            if (player != null && playerRb != null && !player.groundDetected)
            {
                // 清空玩家当前的下坠速度
                playerRb.linearVelocity = Vector2.zero;

                // 【手感优化1】：如果玩家正处于怪物的正上方 (X轴偏移极小)
                // 我们强行给它一个水平力，防止出现“径直往上飞”的尴尬情况
                if (Mathf.Abs(finalRecoilDir.x) < 0.3f)
                {
                    // 默认将玩家往“角色当前朝向的背后”反推
                    finalRecoilDir.x = -player.facingDir;
                }

                // 【手感优化2】：允许玩家在弹反瞬间按左右方向键，主动改变跑酷的位移方向
                if (player.moveInput.x != 0)
                {
                    // 玩家按哪边，就给反冲力附加哪边的权重（让空中跑酷极其可控）
                    finalRecoilDir.x += player.moveInput.x * 0.8f;
                }

                // 保证垂直方向上有一个最小的向上推力
                finalRecoilDir.y = Mathf.Abs(finalRecoilDir.y) + minUpwardBoost;

                // 重新归一化向量，确保力的大小均衡
                finalRecoilDir = finalRecoilDir.normalized;

                // 给玩家自身施加爆发反冲力
                playerRb.AddForce(finalRecoilDir * selfPushForce, ForceMode2D.Impulse);
            }
        }

        return hasCounteredSomeone;
    }

    public float GetCounterRecoveryDuration() => counterRecovery;

    // 在编辑器中画出光圈范围，方便你直观地调整 counterRadius 数值
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, counterRadius);
    }

}