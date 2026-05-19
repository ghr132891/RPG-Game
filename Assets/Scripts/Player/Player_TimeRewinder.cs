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
    public float recordTime = 3f;
    public float rewindPlaybackDuration = 3f;

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

        // 【终极防御 1】：强制保证至少有 3 秒的播放时间（防止面板数据被覆盖）
        float actualDuration = Mathf.Max(3f, rewindPlaybackDuration);
        Debug.Log($"<color=cyan>【时间回溯开始】开启护城河模式，预计持续时间: {actualDuration} 秒</color>");

        // 首次开启特效
        if (WorldFilterManager.Instance != null)
        {
            WorldFilterManager.Instance.ToggleTimeRewindEffect(true);
        }

        // 冻结状态机、物理、碰撞器、动画流速
        player.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        player.anim.speed = 0f;

        float timer = 0f;
        int totalPoints = pointsInTime.Count;

        while (timer < actualDuration)
        {
            timer += Time.unscaledDeltaTime;

            // ==========================================
            // 【终极防御 2】：滤镜续命！
            // 防止法师切换状态或场景重置时把特效意外关掉。
            // 只要我还在回溯，谁也别想黑掉我的滤镜！
            // ==========================================
            if (WorldFilterManager.Instance != null)
            {
                if (WorldFilterManager.Instance.transitionScreen != null)
                    WorldFilterManager.Instance.transitionScreen.enabled = true; // 死死保住 UI
                if (WorldFilterManager.Instance.timeWorldVolume != null)
                    WorldFilterManager.Instance.timeWorldVolume.weight = 1f;     // 死死保住后处理
            }

            float t = Mathf.Clamp01(timer / actualDuration);
            float floatIndex = Mathf.Lerp(0, totalPoints - 1, t);

            int indexA = Mathf.FloorToInt(floatIndex);
            int indexB = Mathf.Min(indexA + 1, totalPoints - 1);

            float fraction = floatIndex - indexA;

            // 还原物理位置
            Vector3 smoothPosition = Vector3.Lerp(pointsInTime[indexA].position, pointsInTime[indexB].position, fraction);
            rb.MovePosition(smoothPosition);

            // 还原玩家朝向
            if (player.facingDir != pointsInTime[indexA].facingDir)
            {
                player.Flip();
            }

            // 还原全姿态动画
            player.anim.Play(pointsInTime[indexA].animStateHash, 0, pointsInTime[indexA].animNormalizedTime);

            yield return null;
        }

        Debug.Log("<color=cyan>【时间回溯结束】完成完整倒放！</color>");

        // 精确收尾
        if (totalPoints > 0)
        {
            rb.MovePosition(pointsInTime[totalPoints - 1].position);
            if (player.facingDir != pointsInTime[totalPoints - 1].facingDir) player.Flip();
            player.anim.Play(pointsInTime[totalPoints - 1].animStateHash, 0, pointsInTime[totalPoints - 1].animNormalizedTime);
        }

        pointsInTime.Clear();

        // 恢复控制与物理
        rb.bodyType = RigidbodyType2D.Dynamic;
        if (col != null) col.enabled = true;
        player.anim.speed = 1f;

        player.enabled = true;
        isRewinding = false;

        // 倒放彻底结束，才允许关闭特效
        if (WorldFilterManager.Instance != null)
        {
            WorldFilterManager.Instance.ToggleTimeRewindEffect(false);
        }
    }
}