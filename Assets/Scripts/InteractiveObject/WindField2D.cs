using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WindField2D : MonoBehaviour
{
    [Header("风场核心设置")]
    [Tooltip("玩家在风场中上升的最大速度")]
    [SerializeField] private float maxRiseVelocity = 8f;

    [Tooltip("速度变化的柔和度（值越小越有延迟的‘浮力感’，值越大越快达到最高速）")]
    [SerializeField] private float velocityLerpFactor = 5f;

    [Header("悬停优雅平滑设置")]
    [Tooltip("距离风场顶部多远的距离开始减速")]
    [SerializeField] private float decelerationRange = 2.0f;

    [Tooltip("判定到达顶部的偏置距离")]
    [SerializeField] private float hoverOffsetFromTop = 0.5f;

    [Tooltip("到达风场顶部后的缓慢下降速度（负数代表向下）")]
    [SerializeField] private float topDescentSpeed = -2.0f;

    [Header("视觉表现")]
    [Tooltip("关联子物体的粒子系统，代码会自动调整其形状匹配碰撞体")]
    [SerializeField] private ParticleSystem windParticles;

    private BoxCollider2D windCollider;
    private float originalGravityScale = -1f;

    private void Awake()
    {
        windCollider = GetComponent<BoxCollider2D>();
        windCollider.isTrigger = true;
        SyncParticleShape();
    }

    private void OnValidate()
    {
        if (windCollider == null) windCollider = GetComponent<BoxCollider2D>();
        SyncParticleShape();
    }

    private void SyncParticleShape()
    {
        if (windParticles != null && windCollider != null)
        {
            var shape = windParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(windCollider.size.x, 0f, 1f);

            float bottomY = windCollider.offset.y - (windCollider.size.y / 2f);
            windParticles.transform.localPosition = new Vector3(windCollider.offset.x, bottomY, 0f);

            var main = windParticles.main;
            float lifeTime = windCollider.size.y / main.startSpeed.constant;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeTime * 0.8f, lifeTime * 1.1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null && player.rb != null)
        {
            if (originalGravityScale < 0) originalGravityScale = player.rb.gravityScale;
            if (player.isPlunging) return;

            // 进风场时不强制给一半速度，而是保持现有速度，让 Lerp 去接管，更加顺滑
            player.rb.gravityScale = 0f;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null && player.rb != null)
        {
            if (player.isPlunging)
            {
                if (originalGravityScale >= 0) player.rb.gravityScale = originalGravityScale;
                return;
            }
            player.rb.gravityScale = 0f;
            HandlePlayerWindPhysics(player);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null && player.rb != null)
        {
            if (originalGravityScale >= 0) player.rb.gravityScale = originalGravityScale;
            originalGravityScale = -1f;
        }
    }

    private void HandlePlayerWindPhysics(Player player)
    {
        Rigidbody2D rb = player.rb;

        float topCeilingY = windCollider.bounds.max.y - hoverOffsetFromTop;
        float currentDistanceToTop = topCeilingY - player.transform.position.y;

        float targetYVelocity;

        // 1. 确定目标速度（Target Velocity）
        if (currentDistanceToTop <= 0)
        {
            // 【核心修改2：缓慢下降】
            // 如果玩家到达或顶出了风场最高点，取消之前的硬锁定，而是把目标速度设为“向下缓慢降落”
            targetYVelocity = topDescentSpeed;
        }
        else
        {
            // 如果在风场内部，按曲线计算向上的推力
            float t = Mathf.Clamp01(currentDistanceToTop / decelerationRange);
            float smoothFactor = t * t * (3f - 2f * t); // SmoothStep 曲线
            targetYVelocity = maxRiseVelocity * smoothFactor;
        }

        // 2. 【核心修改1：极其柔和的流体插值】
        // 放弃僵硬的 MoveTowards，改用 Mathf.Lerp。
        // 当实际速度向目标速度变化时，Lerp 会随着差值变小而逐渐减慢加速度。
        // 这完美模拟了流体动力学（空气阻力），起飞时推力大，越接近目标状态越柔和。
        float newYVelocity = Mathf.Lerp(rb.linearVelocity.y, targetYVelocity, velocityLerpFactor * Time.deltaTime);

        // 应用速度，X轴完全自由
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, newYVelocity);
    }
}