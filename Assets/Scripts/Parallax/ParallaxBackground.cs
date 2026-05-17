using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;

    // 改用 Vector2 记录摄像机的 X 和 Y 坐标
    private Vector2 lastMainCameraPosition;

    private float cameraHalfWidth;
    private float cameraHalfHeight; // 新增：摄像机一半的高度

    [SerializeField] private ParallaxLayer[] backgroundLayers;

    private void Awake()
    {
        mainCamera = Camera.main;

        // 正交摄像机的 orthographicSize 就是它高度的一半
        cameraHalfHeight = mainCamera.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;

        // 初始化记录相机的起始位置
        lastMainCameraPosition = mainCamera.transform.position;

        CalculateBackgroundSize();
    }

    private void LateUpdate()
    {
        Vector2 currentCameraPosition = mainCamera.transform.position;

        // 获取 X 和 Y 轴的移动距离
        Vector2 distanceToMove = currentCameraPosition - lastMainCameraPosition;
        lastMainCameraPosition = currentCameraPosition;

        // 计算摄像机视野的四个边缘
        float cameraLeftEdge = currentCameraPosition.x - cameraHalfWidth;
        float cameraRightEdge = currentCameraPosition.x + cameraHalfWidth;
        float cameraBottomEdge = currentCameraPosition.y - cameraHalfHeight;
        float cameraTopEdge = currentCameraPosition.y + cameraHalfHeight;

        foreach (ParallaxLayer layer in backgroundLayers)
        {
            // 将 X和Y 的移动距离传进去
            layer.Move(distanceToMove);
            // 将四个边缘传进去判断循环
            layer.LoopBackground(cameraLeftEdge, cameraRightEdge, cameraBottomEdge, cameraTopEdge);
        }
    }

    private void CalculateBackgroundSize()
    {
        foreach (ParallaxLayer layer in backgroundLayers)
        {
            layer.CalculateImageSize();
        }
    }
}
