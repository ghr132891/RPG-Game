using System.Collections.Generic;
using UnityEngine;

public struct PointInTime
{
    public Vector3 localPosition;
    public Vector2 velocity;
    public int facingDir;
}

public class TimeRewinder : MonoBehaviour
{
    [Header("倒流设置")]
    public float recordTime = 5f;
    private List<PointInTime> pointsInTime;
    private Rigidbody2D rb;
    private Entity entity;

    private bool isDead;
    private bool isRewinding = false;

    // 【新增】：用于记录真实的刚体类型，避免 isKinematic 警告
    private RigidbodyType2D originalBodyType;

    void Start()
    {
        pointsInTime = new List<PointInTime>();
        rb = GetComponent<Rigidbody2D>();
        entity = GetComponent<Entity>();

        if (WorldManager.Instance != null)
            WorldManager.Instance.OnWorldChanged += CheckTimeWorld;
    }

    private void CheckTimeWorld(WorldType worldType)
    {
        if (isDead)
            return;

        if (worldType == WorldType.Time)
            StartRewind();
        else
            StopRewind();
    }

    void FixedUpdate()
    {
        // 【核心修复2：死亡判定】
        // 这里需要对接你项目的真实死亡判定。
        // 根据你之前的代码架构，你的 Entity 应该有类似 isDead 的属性，或者你可以通过判断它的状态
        // 伪代码：如果 (entity.isDead == true) 或者 (血量 <= 0)
        // 请把下方判断换成你项目中真实的死亡检测！比如 entity.stats.isDead 或者 GetComponent<Enemy_Health>().currentHealth <= 0
        //if (entity != null && entity.isKnocked == false) // 假设我们先用伪逻辑占位
        //{
            // 如果你需要具体的死亡判断代码，告诉我你的 Entity.cs 里是怎么写死亡的！
        //}

        // 如果已经判定死亡，直接退出，不记录也不倒流
        if (isDead) return;

        if (isRewinding)
            Rewind();
        else
            Record();
    }

    public void SetDead()
    {
        isDead = true;
        pointsInTime.Clear(); // 清空历史记录
        if (rb != null)
            rb.bodyType = originalBodyType; // 恢复物理状态
    }

    void Rewind()
    {
        if (pointsInTime.Count > 0)
        {
            PointInTime pointInTime = pointsInTime[0];

            transform.localPosition = pointInTime.localPosition;
            if (entity != null && pointInTime.facingDir != entity.facingDir)
            {
                entity.Flip();
            }

            pointsInTime.RemoveAt(0);
        }
        else
        {
            // 【修复】：使用 bodyType 替代 isKinematic
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    void Record()
    {
        if (pointsInTime.Count > Mathf.Round(recordTime / Time.fixedDeltaTime))
        {
            pointsInTime.RemoveAt(pointsInTime.Count - 1);
        }

        PointInTime point = new PointInTime();
        point.localPosition = transform.localPosition;
        point.velocity = rb != null ? rb.linearVelocity : Vector2.zero;
        point.facingDir = entity != null ? entity.facingDir : 1;

        pointsInTime.Insert(0, point);
    }

    public void StartRewind()
    {
        if (isDead)
            return;

        isRewinding = true;
        // 【修复】：记录原始状态，并使用 bodyType 替代 isKinematic
        if (rb != null)
        {
            originalBodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        if (entity != null) entity.enabled = false;
    }

    public void StopRewind()
    {
        isRewinding = false;
        // 【修复】：还原原始的 bodyType
        if (rb != null)
        {
            rb.bodyType = originalBodyType;
            if (pointsInTime.Count > 0) rb.linearVelocity = pointsInTime[0].velocity;
        }
        if (entity != null) entity.enabled = true;
    }

    private void OnDestroy()
    {
        if (WorldManager.Instance != null)
            WorldManager.Instance.OnWorldChanged -= CheckTimeWorld;
    }
}