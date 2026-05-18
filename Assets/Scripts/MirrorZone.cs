using UnityEngine;
using UnityEngine.Tilemaps;

public class MirrorZone : MonoBehaviour
{
    private bool isCurrentlyMirrored = false;

    [Header("调试")]
    [Tooltip("这里会显示代码自动算出来的实际贴图中心X坐标（本地坐标）")]
    public float calculatedAxisX = 0f;

    private void Start()
    {
        // 游戏开始时，自动计算已画贴图的真实中心
        CalculateRealTilemapCenter();

        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnWorldChanged += HandleWorldChanged;

            if (WorldManager.Instance.isMirrored)
            {
                ExecuteFlip();
            }
        }
    }

    // ================= 新增：核心计算逻辑 =================
    private void CalculateRealTilemapCenter()
    {
        // 获取房间内所有的 Tilemap (比如可能分了地面层、墙壁层、背景层)
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
            // 【关键步骤】强制 Tilemap 丢弃之前画错又擦除的空白区域，完美贴合现有砖块
            tm.CompressBounds();

            // 获取这个 Tilemap 的渲染器
            TilemapRenderer renderer = tm.GetComponent<TilemapRenderer>();
            if (renderer != null)
            {
                if (isFirst)
                {
                    combinedBounds = renderer.bounds; // 记录第一个边界 (世界坐标)
                    isFirst = false;
                }
                else
                {
                    combinedBounds.Encapsulate(renderer.bounds); // 把其他层的边界包进来，合并成一个大边界
                }
            }
        }

        // combinedBounds.center 算出来的是“世界坐标系”下的正中心。
        // 因为你的翻转代码是用的 localPosition，所以我们要把它转换成本地坐标X
        Vector3 localCenter = transform.InverseTransformPoint(combinedBounds.center);
        calculatedAxisX = localCenter.x;

        Debug.Log($"[{gameObject.name}] 成功计算出贴图真实中心，本地X轴为: {calculatedAxisX}");
    }
    // ======================================================

    private void HandleWorldChanged(WorldType newWorld)
    {
        bool targetMirrored = WorldManager.Instance.isMirrored;
        if (isCurrentlyMirrored != targetMirrored)
        {
            ExecuteFlip();
        }
    }

    private void ExecuteFlip()
    {
        isCurrentlyMirrored = !isCurrentlyMirrored;

        foreach (Transform child in transform)
        {
            // 1. 物理位置翻转：以我们算出来的 calculatedAxisX 为基准对称翻转
            float newX = (2 * calculatedAxisX) - child.localPosition.x;
            child.localPosition = new Vector3(newX, child.localPosition.y, child.localPosition.z);

            // 2. 状态与朝向翻转
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
                // 注意：如果普通物件是 Tilemap 本身，这里翻转它的 Scale
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

    // ================= 新增：画线辅助，不用运行游戏就能看到中心在哪 =================
    private void OnDrawGizmosSelected()
    {
        // 哪怕没运行游戏，只要你选中了这个房间，它就会尝试算一下中心并在场景里画出来
        CalculateRealTilemapCenter();

        Gizmos.color = Color.green;

        // 算出世界坐标的中心轴位置
        Vector3 worldCenterPoint = transform.TransformPoint(new Vector3(calculatedAxisX, 0, 0));

        // 画一条绿色的垂直辅助线
        Vector3 top = worldCenterPoint + new Vector3(0, 20f, 0);
        Vector3 bottom = worldCenterPoint + new Vector3(0, -20f, 0);
        Gizmos.DrawLine(top, bottom);

        // 画一个小球标记中心
        Gizmos.DrawSphere(worldCenterPoint, 0.3f);
    }
}