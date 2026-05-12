using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerPointInTime
{
    public Vector3 position;
}

public class Player_TimeRewinder : MonoBehaviour
{
    private Player player;
    private Rigidbody2D rb;

    [Header("Rewind Settings")]
    public float recordTime = 5f;

    // 【新增】：倒放过程消耗的真实时间（越小退得越快，但绝对平滑）
    public float rewindPlaybackDuration = 1.5f;

    public bool isRewinding { get; private set; }

    private List<PlayerPointInTime> pointsInTime;

    void Start()
    {
        player = GetComponent<Player>();
        rb = GetComponent<Rigidbody2D>();
        pointsInTime = new List<PlayerPointInTime>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            StartRewind();

    }

    void FixedUpdate()
    {
        if (isRewinding) return;
        Record();
    }

    private void Record()
    {
        pointsInTime.Insert(0, new PlayerPointInTime { position = transform.position });
        int maxFrames = Mathf.RoundToInt(recordTime / Time.fixedDeltaTime);
        if (pointsInTime.Count > maxFrames)
        {
            pointsInTime.RemoveAt(pointsInTime.Count - 1);
        }
    }

    public void StartRewind()
    {
        if (pointsInTime.Count == 0) return;

        if (!isRewinding)
        {
            StartCoroutine(RewindRoutine());
        }
    }

    private IEnumerator RewindRoutine()
    {
        isRewinding = true;

        // 1. 冻结状态机与物理
        player.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 2. 【核心修改：丝滑插值倒放】
        float timer = 0f;
        int totalPoints = pointsInTime.Count;

        // 使用普通的 yield return null 配合 Time.deltaTime，让画面刷新率决定顺滑度
        while (timer < rewindPlaybackDuration)
        {
            timer += Time.deltaTime;

            // 计算当前时间进度百分比 (0 到 1)
            float t = Mathf.Clamp01(timer / rewindPlaybackDuration);

            // 根据百分比，映射到记录列表的“虚拟浮点索引”上
            float floatIndex = Mathf.Lerp(0, totalPoints - 1, t);

            // 找到离这个浮点索引最近的两个真实坐标点（A点和B点）
            int indexA = Mathf.FloorToInt(floatIndex);
            int indexB = Mathf.Min(indexA + 1, totalPoints - 1);

            // 计算在A点和B点之间的微小偏移除数
            float fraction = floatIndex - indexA;

            // 使用 Lerp 完美计算出玩家此时应该在的精确平滑位置！
            Vector3 smoothPosition = Vector3.Lerp(pointsInTime[indexA].position, pointsInTime[indexB].position, fraction);

            rb.MovePosition(smoothPosition);

            yield return null;
        }

        // 3. 精确收尾
        if (totalPoints > 0)
        {
            rb.MovePosition(pointsInTime[totalPoints - 1].position);
        }

        pointsInTime.Clear();

        // 4. 恢复控制
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (col != null) col.enabled = true;
        player.enabled = true;
        isRewinding = false;
    }
}