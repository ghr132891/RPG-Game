using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))] // 强制要求挂载碰撞体
public class MirrorZone : MonoBehaviour
{
    private bool isCurrentlyMirrored = false;

    // 【新增】：用于记录玩家是否在当前镜像区域内
    private bool isPlayerInside = false;

    [Header("调试")]
    [Tooltip("这里会显示代码自动算出来的实际贴图中心X坐标（本地坐标）")]
    public float calculatedAxisX = 0f;

    private void Start()
    {
        // 确保挂在身上的 Collider 是 Trigger 模式
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // 游戏开始时，自动计算已画贴图的真实中心
        CalculateRealTilemapCenter();

        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnWorldChanged += HandleWorldChanged;

            // 初始化时如果已经是镜像状态，也检查一下（通常游戏开始是 Normal）
            if (WorldManager.Instance.isMirrored && isPlayerInside)
            {
                ExecuteFlip();
            }
        }
    }

    // ================= 新增：检测玩家进出区域 =================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测进入的是否是玩家
        if (collision.GetComponent<Player>() != null)
        {
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 检测离开的是否是玩家
        if (collision.GetComponent<Player>() != null)
        {
            isPlayerInside = false;
        }
    }
    // ==========================================================

    private void CalculateRealTilemapCenter()
    {
        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();

        if (tilemaps.Length == 0)
        {
            Debug.LogWarning("房间内没有找到Tilemap！将以父物体原点为中心。");
            calculatedAxisX = 0f;
            return;
        }

        Bounds combinedBounds = new Bounds();
        bool isFirst = true;

        foreach (Tilemap tm in tilemaps)
        {
            tm.CompressBounds();
            TilemapRenderer renderer = tm.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                if (isFirst)
                {
                    combinedBounds = renderer.bounds;
                    isFirst = false;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds);
                }
            }
        }

        Vector3 localCenter = transform.InverseTransformPoint(combinedBounds.center);
        calculatedAxisX = localCenter.x;
    }

    // ================= 修改：只在玩家在内部时才镜像 =================
    private void HandleWorldChanged(WorldType newWorld)
    {
        bool globalMirrored = WorldManager.Instance.isMirrored;

        if (globalMirrored)
        {
            // 全局切为镜像：只有玩家在区域内，且区域还没被翻转，才执行翻转
            if (isPlayerInside && !isCurrentlyMirrored)
            {
                ExecuteFlip();
            }
        }
        else
        {
            // 全局切为正常/时间：如果当前区域之前被翻转了，那么强制恢复（无论玩家在不在里面）
            if (isCurrentlyMirrored)
            {
                ExecuteFlip();
            }
        }
    }
    // ================================================================

    private void ExecuteFlip()
    {
        isCurrentlyMirrored = !isCurrentlyMirrored;

        foreach (Transform child in transform)
        {
            float newX = (2 * calculatedAxisX) - child.localPosition.x;
            child.localPosition = new Vector3(newX, child.localPosition.y, child.localPosition.z);

            Entity entity = child.GetComponent<Entity>();
            if (entity != null)
            {
                entity.Flip();

                Rigidbody2D rb = child.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);
                }
            }
            else
            {
                child.localScale = new Vector3(-child.localScale.x, child.localScale.y, child.localScale.z);
            }
        }
    }

    private void OnDestroy()
    {
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnWorldChanged -= HandleWorldChanged;
        }
    }

    private void OnDrawGizmosSelected()
    {
        CalculateRealTilemapCenter();
        Gizmos.color = Color.green;
        Vector3 worldCenterPoint = transform.TransformPoint(new Vector3(calculatedAxisX, 0, 0));
        Vector3 top = worldCenterPoint + new Vector3(0, 20f, 0);
        Vector3 bottom = worldCenterPoint + new Vector3(0, -20f, 0);
        Gizmos.DrawLine(top, bottom);
        Gizmos.DrawSphere(worldCenterPoint, 0.3f);
    }
}