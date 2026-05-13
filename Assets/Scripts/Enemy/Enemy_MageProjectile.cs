using UnityEngine;

public class Enemy_MageProjectile : MonoBehaviour, ICounterable
{
    private Entity_Combat combat;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;

    [Header("Flight Settings")]
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private LayerMask whatCanCollideWith;

    // 【新增】：速度倍率控制器！数值越小，飞得越慢。
    [Tooltip("投掷速度倍率：1为原速，0.5为速度减半。数值越小飞得越慢")]
    [SerializeField][Range(0.1f, 3f)] private float flightSpeedMultiplier = 0.6f;

    // === 弹反重写专用变量 ===
    [Header("Parry Settings")]
    private bool isParried = false;
    private Vector2 parryStartPos;
    private float parryTimer;
    [SerializeField] private float parryFlightDuration = 0.4f; // 飞回去所需的时间（越小飞得越快）

    public bool CanBeCountered => true;

    public void SetupProjectile(Transform target, Entity_Combat combat)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();

        anim.enabled = false;
        this.combat = combat;
        isParried = false;

        // 【关键修改】：要让抛物线变慢，且不改变弧度高度，必须动态降低重力！
        // 物理学公式：速度变为原来的 N 倍，重力需要变为原来的 N 的平方倍。
        float baseGravityScale = rb.gravityScale; // 获取预制体原本的重力倍率
        rb.gravityScale = baseGravityScale * (flightSpeedMultiplier * flightSpeedMultiplier);

        // 法师发射时，基于新的慢速重力，计算出需要的慢速初速度
        Vector2 velocity = CalculateBallisticVelocity(transform.position, target.position);
        rb.linearVelocity = velocity;
    }

    private void Update()
    {
        // === 重写的弹反飞行逻辑 ===
        if (isParried)
        {
            // 如果飞行途中法师被其他东西打死了，魔法球原地销毁
            if (combat == null || !combat.gameObject.activeInHierarchy)
            {
                Explode();
                return;
            }

            // 计时器
            parryTimer += Time.deltaTime;
            float t = Mathf.Clamp01(parryTimer / parryFlightDuration);

            // A: 起点（弹反发生的位置）
            Vector2 p0 = parryStartPos;
            // C: 终点（实时获取法师的胸口位置，自带动态追踪！）
            Vector2 p2 = combat.transform.position + Vector3.up * 1.0f;
            // B: 控制点（决定抛物线的高度，取中点并向上偏移 arcHeight）
            Vector2 p1 = p0 + (p2 - p0) / 2f + Vector2.up * arcHeight;

            // 二次贝塞尔曲线数学公式（完美平滑的抛物线）
            transform.position = Mathf.Pow(1 - t, 2) * p0 +
                                 2 * t * (1 - t) * p1 +
                                 Mathf.Pow(t, 2) * p2;

            // 当 t >= 1 时，代表精确到达法师身上
            if (t >= 1f)
            {
                combat.PerformAttackOnTarget(combat.transform);
                Explode();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 弹反状态下，无视一切原有的碰撞判定！全权交由 Update 里的到达判定
        if (isParried) return;

        if (((1 << collision.gameObject.layer) & whatCanCollideWith) != 0)
        {
            combat.PerformAttackOnTarget(collision.transform);
            Explode();
        }
    }

    // 提取出的公共爆炸销毁方法
    private void Explode()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        anim.enabled = true;
        col.enabled = false;
        this.enabled = false; // 停止 Update 计算
        Destroy(gameObject, 2);
    }

    // 你原汁原味的法师发射公式
    private Vector2 CalculateBallisticVelocity(Vector2 start, Vector2 end)
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        if (gravity == 0) gravity = 9.8f; // 保底防止除数为0报错

        float displacementY = end.y - start.y;
        float displacementX = end.x - start.x;

        float peakHieght = Mathf.Max(arcHeight, end.y - start.y + .1f);

        float timeToApex = Mathf.Sqrt(2 * peakHieght / gravity);
        float timeFromApex = Mathf.Sqrt(2 * (peakHieght - displacementY) / gravity);
        float totalTime = timeToApex + timeFromApex;

        float velocityY = Mathf.Sqrt(2 * gravity * peakHieght);
        float velocityX = displacementX / totalTime;

        return new Vector2(velocityX, velocityY);
    }

    // === 弹反入口 ===
    public void HandleCounter()
    {
        if (isParried) return;

        isParried = true;

        if (combat != null)
        {
            // 通知法师，这个炸弹被成功弹反了！
            Enemy_Mage mage = combat.GetComponent<Enemy_Mage>();
            if (mage != null)
            {
                mage.OnBombParried();
            }
        }

        parryStartPos = transform.position;
        parryTimer = 0f;

        // 把刚体变成运动学（Kinematic），彻底关闭物理引擎对它的影响！
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // 关闭物理碰撞器，防止半路撞墙、撞地提前销毁
        col.enabled = false;

        // 翻转贴图
        transform.Rotate(0, 180, 0);
    }
}