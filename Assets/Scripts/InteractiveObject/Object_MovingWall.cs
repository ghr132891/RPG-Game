using UnityEngine;

public class Object_MovingWall : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("墙体移动的速度")]
    public float moveSpeed = 3f;
    [Tooltip("打开时移动的相对距离(例如 Y设为 3 代表向上升 3个单位)")]
    public Vector3 openOffset = new Vector3(0, 3f, 0);

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Vector3 targetPosition;

    private void Awake()
    {
        // 记录初始位置作为关闭状态的位置
        closedPosition = transform.position;
        // 计算打开状态的位置
        openPosition = closedPosition + openOffset;
        // 初始目标为关闭位置
        targetPosition = closedPosition;
    }

    private void Update()
    {
        // 平滑移动到目标位置
        if (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    // 由外界（如压力板）调用
    public void OpenWall()
    {
        targetPosition = openPosition;
    }

    // 由外界调用
    public void CloseWall()
    {
        targetPosition = closedPosition;
    }
}
