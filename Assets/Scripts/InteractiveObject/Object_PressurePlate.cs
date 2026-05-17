using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Object_PressurePlate : MonoBehaviour
{
    [Header("机关连接")]
    [Tooltip("踩下时要触发的墙体/机关")]
    public Object_MovingWall targetWall;

    [Header("视觉反馈")]
    [Tooltip("踩下时视觉向下偏移的距离")]
    public float sinkDistance = 0.15f;
    [Tooltip("下沉的视觉子物体(把SpriteRenderer放子物体拖到这里)")]
    public Transform visualTransform;

    private Vector3 originalVisualPos;
    private int objectsOnPlate = 0; // 记录当前有几个物体压在上面

    private void Awake()
    {
        if (visualTransform != null)
        {
            originalVisualPos = visualTransform.localPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 【核心修改】：把 GetComponent<Player> 改为 GetComponent<Entity>
        // 因为 Player 和 Enemy 都继承自 Entity，所以现在玩家和怪物都能触发！
        Entity entity = collision.GetComponent<Entity>();

        if (entity != null)
        {
            objectsOnPlate++;

            // 只要有1个物体压上来，就触发机关
            if (objectsOnPlate == 1)
            {
                ActivatePlate();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 【核心修改】：同样改为 Entity
        Entity entity = collision.GetComponent<Entity>();

        if (entity != null)
        {
            objectsOnPlate--;

            if (objectsOnPlate < 0) objectsOnPlate = 0;

            // 当没有任何物体压在上面时，机关恢复
            if (objectsOnPlate == 0)
            {
                DeactivatePlate();
            }
        }
    }

    private void ActivatePlate()
    {
        // 1. 播放机关音效
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("plate_press"); // 替换为你的音效名

        // 2. 视觉下沉
        if (visualTransform != null)
        {
            visualTransform.localPosition = originalVisualPos + new Vector3(0, -sinkDistance, 0);
        }

        // 3. 通知墙体打开
        if (targetWall != null)
        {
            targetWall.OpenWall();
        }
    }

    private void DeactivatePlate()
    {
        // 1. 播放恢复音效 (可选)
        if (AudioManager.instance != null)
            AudioManager.instance.PlayGlobalSFX("plate_release");

        // 2. 视觉恢复
        if (visualTransform != null)
        {
            visualTransform.localPosition = originalVisualPos;
        }

        // 3. 通知墙体关闭
        if (targetWall != null)
        {
            targetWall.CloseWall();
        }
    }
}
