using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    [SerializeField] private Transform background;

    [Header("视差移动倍率 (X=左右, Y=上下)")]
    // 改为 Vector2，这样可以分别设置上下和左右的视差速度
    [SerializeField] private Vector2 parallaxMultiplier;

    [Header("是否开启无限循环")]
    [SerializeField] private bool loopX = true;
    [SerializeField] private bool loopY = true;

    // 用 Vector2 存储宽高数据，更加简洁
    private Vector2 imageFullSize;
    private Vector2 imageHalfSize;

    [SerializeField] private float imageWidthOffset = 10;
    [SerializeField] private float imageHeightOffset = 10; // 新增 Y 轴偏移容错

    public void CalculateImageSize()
    {
        // 直接获取 bounds 的 size (包含 x 宽 和 y 高)
        imageFullSize = background.GetComponent<SpriteRenderer>().bounds.size;
        imageHalfSize = imageFullSize / 2;
    }

    public void Move(Vector2 distanceToMove)
    {
        // 根据乘数，分别在 X 和 Y 轴上移动背景图
        Vector3 movement = new Vector3(
            distanceToMove.x * parallaxMultiplier.x,
            distanceToMove.y * parallaxMultiplier.y,
            0
        );
        background.position += movement;
    }

    public void LoopBackground(float cameraLeftEdge, float cameraRightEdge, float cameraBottomEdge, float cameraTopEdge)
    {
        // ------------------ 处理 X 轴 (左右循环) ------------------
        if (loopX)
        {
            float imageRightEdge = (background.position.x + imageHalfSize.x) - imageWidthOffset;
            float imageLeftEdge = (background.position.x - imageHalfSize.x) + imageWidthOffset;

            // 被摄像机甩在了左边，把它拉到右边来
            if (imageRightEdge < cameraLeftEdge)
                background.position += Vector3.right * imageFullSize.x;
            // 被摄像机甩在了右边，把它拉到左边来
            else if (imageLeftEdge > cameraRightEdge)
                background.position += Vector3.right * -imageFullSize.x;
        }

        // ------------------ 处理 Y 轴 (上下循环) ------------------
        if (loopY)
        {
            float imageTopEdge = (background.position.y + imageHalfSize.y) - imageHeightOffset;
            float imageBottomEdge = (background.position.y - imageHalfSize.y) + imageHeightOffset;

            // 被摄像机甩在了下面，把它拉到上面来
            if (imageTopEdge < cameraBottomEdge)
                background.position += Vector3.up * imageFullSize.y;
            // 被摄像机甩在了上面，把它拉到下面来
            else if (imageBottomEdge > cameraTopEdge)
                background.position += Vector3.up * -imageFullSize.y;
        }
    }
}