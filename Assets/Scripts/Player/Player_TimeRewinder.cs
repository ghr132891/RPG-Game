using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. 【扩展记录结构】：加入朝向和动画的精准数据
public struct PlayerPointInTime
{
    public Vector3 position;
    public int facingDir;          // 玩家朝向
    public int animStateHash;      // 当前动画状态的哈希值（是跳跃、攻击还是空闲？）
    public float animNormalizedTime; // 动画播放到的具体进度（0到1+）
}

public class Player_TimeRewinder : MonoBehaviour
{
    private Player player;
    private Rigidbody2D rb;

    [Header("Rewind Settings")]
    public float recordTime = 5f;
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
        // 2. 【核心获取】：抓取动画器第 0 层的当前状态信息
        AnimatorStateInfo animState = player.anim.GetCurrentAnimatorStateInfo(0);

        pointsInTime.Insert(0, new PlayerPointInTime
        {
            position = transform.position,
            facingDir = player.facingDir,
            animStateHash = animState.fullPathHash,
            animNormalizedTime = animState.normalizedTime
        });

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

        // 冻结状态机与物理
        player.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 3. 【冻结动画】：暂停 Animator 自带的时间流逝，完全由我们手动控制！
        player.anim.speed = 0f;

        float timer = 0f;
        int totalPoints = pointsInTime.Count;

        while (timer < rewindPlaybackDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / rewindPlaybackDuration);
            float floatIndex = Mathf.Lerp(0, totalPoints - 1, t);

            int indexA = Mathf.FloorToInt(floatIndex);
            int indexB = Mathf.Min(indexA + 1, totalPoints - 1);

            float fraction = floatIndex - indexA;

            // --- 还原物理位置 ---
            Vector3 smoothPosition = Vector3.Lerp(pointsInTime[indexA].position, pointsInTime[indexB].position, fraction);
            rb.MovePosition(smoothPosition);

            // --- 4. 还原玩家朝向 ---
            if (player.facingDir != pointsInTime[indexA].facingDir)
            {
                player.Flip(); // 调用 Entity 基类的 Flip 方法来翻转人物
            }

            // --- 5. 还原全姿态动画！ ---
            // 强行指定 Animator 播放特定的哈希状态，并精确定位到当时的时间点
            player.anim.Play(pointsInTime[indexA].animStateHash, 0, pointsInTime[indexA].animNormalizedTime);

            yield return null;
        }

        // 精确收尾
        if (totalPoints > 0)
        {
            rb.MovePosition(pointsInTime[totalPoints - 1].position);

            // 恢复最后时刻的朝向和动画
            if (player.facingDir != pointsInTime[totalPoints - 1].facingDir) player.Flip();
            player.anim.Play(pointsInTime[totalPoints - 1].animStateHash, 0, pointsInTime[totalPoints - 1].animNormalizedTime);
        }

        pointsInTime.Clear();

        // 恢复控制与物理
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (col != null) col.enabled = true;

        // 6. 【解冻动画】：恢复正常游戏动画播放速率
        player.anim.speed = 1f;

        player.enabled = true;
        isRewinding = false;
    }
}