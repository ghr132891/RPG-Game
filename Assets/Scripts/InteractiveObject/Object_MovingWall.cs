using UnityEngine;

public class Object_MovingWall : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("墙体移动的速度")]
    public float moveSpeed = 3f;
    [Tooltip("打开时移动的相对距离(例如 Y设为 3 代表向上升 3个单位)")]
    public Vector3 openOffset = new Vector3(0, 3f, 0);

    // 【修改为本地坐标】
    private Vector3 closedLocalPos;
    private Vector3 openLocalPos;
    private Vector3 targetLocalPos;

    private Transform anchor;

    private void Awake()
    {
        // 1. 创建一个空物体作为“锚点”
        GameObject anchorObj = new GameObject(gameObject.name + "_Anchor");
        anchor = anchorObj.transform;

        // 2. 将锚点放置在墙体的初始位置，并把它挂在原本墙体的父节点下
        anchor.SetParent(transform.parent);
        anchor.position = transform.position;
        anchor.rotation = transform.rotation;
        anchor.localScale = Vector3.one;

        // 3. 将这堵墙体设为锚点的子物体
        transform.SetParent(anchor);

        // 4. 计算相对锚点的本地坐标
        // 此时由于它刚好挂在原地不动锚点下，它的 localPosition 就是 (0,0,0)
        closedLocalPos = transform.localPosition;
        openLocalPos = closedLocalPos + openOffset;
        targetLocalPos = closedLocalPos;
    }

    private void Update()
    {
        // 【核心修改】：使用 localPosition 进行本地平滑移动
        // 这样不论全局的世界坐标如何被镜像翻转，它都只在锚点内部运动
        if (transform.localPosition != targetLocalPos)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPos, moveSpeed * Time.deltaTime);
        }
    }

    // 由外界（如压力板）调用
    public void OpenWall()
    {
        targetLocalPos = openLocalPos;
    }

    // 由外界调用
    public void CloseWall()
    {
        targetLocalPos = closedLocalPos;
    }
}