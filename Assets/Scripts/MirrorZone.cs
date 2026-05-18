using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Collider2D))]
public class MirrorZone : MonoBehaviour
{
    public bool isCurrentlyMirrored { get; private set; } = false;
    private bool isPlayerInside = false;

    // 【新增】：用于记录倒计时的协程，防止玩家边缘试探
    private Coroutine exitTimerRoutine;

    [Header("调试")]
    public float calculatedAxisX = 0f;

    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        CalculateRealTilemapCenter();

        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnWorldChanged += HandleWorldChanged;

            if (WorldManager.Instance.isMirrored && isPlayerInside)
            {
                ExecuteFlip();
            }
        }
    }

    // ================= 修改：检测玩家进出区域与0.5秒延时 =================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            isPlayerInside = true;

            // 如果玩家在0.5秒内又走回了房间，取消倒计时恢复！
            if (exitTimerRoutine != null)
            {
                StopCoroutine(exitTimerRoutine);
                exitTimerRoutine = null;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            isPlayerInside = false;

            // 玩家离开，开启 0.5 秒倒计时
            if (gameObject.activeInHierarchy)
            {
                exitTimerRoutine = StartCoroutine(RevertAfterDelayRoutine());
            }
        }
    }

    private System.Collections.IEnumerator RevertAfterDelayRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        // 0.5秒后，如果玩家依然不在区域内，且这个房间还没恢复原状
        if (!isPlayerInside && isCurrentlyMirrored)
        {
            // 【核心修复】：必须通知全局管理者把世界切回正常！
            // 这会自动触发下方 HandleWorldChanged 里的恢复逻辑，同时重置 UI 和全局状态
            if (WorldManager.Instance != null && WorldManager.Instance.currentWorld == WorldType.Mirror)
            {
                WorldManager.Instance.SwitchWorld(WorldType.Normal);
            }
            else
            {
                // 兜底逻辑：如果全局已经是正常，但这个房间卡主了，强制自我恢复
                ExecuteFlip();
            }
        }
    }
    // ====================================================================

    private void CalculateRealTilemapCenter()
    {
        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();
        if (tilemaps.Length == 0)
        {
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

    private void HandleWorldChanged(WorldType newWorld)
    {
        bool globalMirrored = WorldManager.Instance.isMirrored;

        if (globalMirrored)
        {
            if (isPlayerInside && !isCurrentlyMirrored)
            {
                ExecuteFlip();
            }
        }
        else
        {
            if (isCurrentlyMirrored)
            {
                ExecuteFlip();
            }
        }
    }

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