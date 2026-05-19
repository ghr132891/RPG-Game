using UnityEngine;

public class KeepTextUnflipped : MonoBehaviour
{
    private Vector3 originalLocalScale;

    private void Awake()
    {
        // 记录文字最开始（正常状态下）的本地缩放值
        originalLocalScale = transform.localScale;
    }

    private void LateUpdate()
    {
        // 如果没有父物体，或者不是挂在其他物体下面，就不需要处理
        if (transform.parent == null) return;

        // 核心逻辑：获取父物体在世界空间中的绝对缩放值
        float parentGlobalScaleX = transform.parent.lossyScale.x;
        Vector3 currentScale = transform.localScale;

        // 如果父物体（Waypoint）被镜像翻转了（世界缩放变成了负数）
        if (parentGlobalScaleX < 0)
        {
            // 负负得正：强制把文字自身的本地 X 缩放也变成负数
            // 这样在画面里它就能负负抵消，变回正向显示！
            currentScale.x = -Mathf.Abs(originalLocalScale.x);
        }
        else
        {
            // 父物体正常时，文字也保持正常的正数缩放
            currentScale.x = Mathf.Abs(originalLocalScale.x);
        }

        transform.localScale = currentScale;
    }
}
