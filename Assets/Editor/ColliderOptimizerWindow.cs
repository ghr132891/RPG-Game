using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ColliderOptimizerWindow : EditorWindow
{
    private float tolerance = 0.05f; // 默认容差值

    // 在 Unity 顶部菜单栏创建入口
    [MenuItem("Tools/AI Auto/批量生成并精简 Polygon Collider")]
    public static void ShowWindow()
    {
        // 弹出设置窗口
        GetWindow<ColliderOptimizerWindow>("碰撞体优化设置");
    }

    private void OnGUI()
    {
        GUILayout.Label("Douglas-Peucker 降采样设置", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // 容差值滑块：允许用户在界面上动态调整
        tolerance = EditorGUILayout.Slider("容差值 (Tolerance)", tolerance, 0.01f, 1.0f);
        EditorGUILayout.HelpBox("容差值越大，剔除的顶点越多，轮廓越平滑；容差值越小，越贴近原图边缘。对于2D平台跳跃地形，推荐在 0.05 - 0.15 之间测试。", MessageType.Info);

        EditorGUILayout.Space();

        // 居中大按钮
        if (GUILayout.Button("开始生成并优化", GUILayout.Height(35)))
        {
            GenerateAndSimplifyColliders();
        }
    }

    private void GenerateAndSimplifyColliders()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("【自动化提示】请先在 Hierarchy 中选中需要处理的节点。");
            return;
        }

        int successCount = 0;
        int totalOriginalVertices = 0;
        int totalSimplifiedVertices = 0;

        foreach (GameObject obj in selectedObjects)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

            if (sr != null && sr.sprite != null)
            {
                PolygonCollider2D collider = obj.GetComponent<PolygonCollider2D>();
                if (collider == null)
                {
                    collider = obj.AddComponent<PolygonCollider2D>();
                }

                int shapeCount = sr.sprite.GetPhysicsShapeCount();
                collider.pathCount = shapeCount;

                List<Vector2> path = new List<Vector2>();
                List<Vector2> simplifiedPath = new List<Vector2>();

                // 遍历每一个闭合路径
                for (int i = 0; i < shapeCount; i++)
                {
                    path.Clear();
                    simplifiedPath.Clear();

                    sr.sprite.GetPhysicsShape(i, path);
                    totalOriginalVertices += path.Count;

                    // 执行道格拉斯-普克算法进行降采样
                    DouglasPeucker(path, tolerance, simplifiedPath);
                    totalSimplifiedVertices += simplifiedPath.Count;

                    // 赋值简化后的顶点
                    collider.SetPath(i, simplifiedPath.ToArray());
                }

                EditorUtility.SetDirty(obj);
                successCount++;
            }
        }

        // 输出优化战报
        if (successCount > 0)
        {
            int savedVertices = totalOriginalVertices - totalSimplifiedVertices;
            Debug.Log($"<color=green>【优化完毕】处理了 {successCount} 个物体。顶点总数从 {totalOriginalVertices} 降至 {totalSimplifiedVertices} (成功剔除了 {savedVertices} 个冗余顶点！)</color>");
        }
    }

    // ================= 以下为 Douglas-Peucker 算法核心数学逻辑 =================

    private void DouglasPeucker(List<Vector2> points, float tolerance, List<Vector2> result)
    {
        if (points == null || points.Count < 3)
        {
            result.AddRange(points);
            return;
        }

        int firstPoint = 0;
        int lastPoint = points.Count - 1;
        List<int> pointIndexsToKeep = new List<int>();

        // 始终保留首尾点
        pointIndexsToKeep.Add(firstPoint);
        pointIndexsToKeep.Add(lastPoint);

        // 闭合路径处理，防止首尾重合导致计算异常
        while (points[firstPoint] == points[lastPoint] && lastPoint > firstPoint)
        {
            lastPoint--;
        }

        SimplifySection(points, firstPoint, lastPoint, tolerance, pointIndexsToKeep);

        // 按原始顺序对保留的索引进行排序
        pointIndexsToKeep.Sort();

        foreach (int index in pointIndexsToKeep)
        {
            result.Add(points[index]);
        }
    }

    private void SimplifySection(List<Vector2> points, int firstPoint, int lastPoint, float tolerance, List<int> pointIndexsToKeep)
    {
        float maxDistance = 0;
        int indexFarthest = 0;

        // 寻找距离首尾连线最远的顶点
        for (int i = firstPoint + 1; i < lastPoint; i++)
        {
            float distance = PerpendicularDistance(points[firstPoint], points[lastPoint], points[i]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexFarthest = i;
            }
        }

        // 如果最远距离大于容差，则保留该点，并递归处理被分割的两段
        if (maxDistance > tolerance && indexFarthest != 0)
        {
            pointIndexsToKeep.Add(indexFarthest);
            SimplifySection(points, firstPoint, indexFarthest, tolerance, pointIndexsToKeep);
            SimplifySection(points, indexFarthest, lastPoint, tolerance, pointIndexsToKeep);
        }
    }

    // 计算点到线段的垂直距离
    private float PerpendicularDistance(Vector2 Point1, Vector2 Point2, Vector2 Point)
    {
        float area = Mathf.Abs(0.5f * (Point1.x * Point2.y + Point2.x * Point.y + Point.x * Point1.y - Point2.x * Point1.y - Point.x * Point2.y - Point1.x * Point.y));
        float bottom = Mathf.Sqrt(Mathf.Pow(Point1.x - Point2.x, 2) + Mathf.Pow(Point1.y - Point2.y, 2));
        return area / bottom * 2.0f;
    }
}
