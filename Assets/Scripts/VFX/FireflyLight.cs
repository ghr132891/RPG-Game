using UnityEngine;
using UnityEngine.Rendering.Universal; // 必须引入 URP 命名空间才能控制 Light2D

[RequireComponent(typeof(Light2D))]
public class FireflyLight : MonoBehaviour
{
    private Light2D fireflyLight;

    [Header("呼吸灯设置")]
    public float baseIntensity = 1.0f;     // 基础亮度
    public float flickerSpeed = 2.0f;      // 闪烁/呼吸的速度
    public float flickerAmount = 0.5f;     // 忽明忽暗的浮动幅度

    private float randomOffset;            // 随机偏移，防止所有萤火虫同频同步闪烁

    private void Start()
    {
        fireflyLight = GetComponent<Light2D>();

        // 给每个萤火虫分配一个随机的初始时间种子
        randomOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        // 使用柏林噪声 (Perlin Noise) 生成非常平滑且有机的随机值 (返回 0 到 1 之间)
        float noise = Mathf.PerlinNoise(randomOffset, Time.time * flickerSpeed);

        // 根据噪声动态计算当前的亮度
        // 映射逻辑：基础亮度 加上 (噪声值 - 0.5) 乘以 浮动幅度
        float currentIntensity = baseIntensity + (noise - 0.5f) * flickerAmount;

        // 限制亮度不能小于0
        fireflyLight.intensity = Mathf.Max(0f, currentIntensity);
    }
}
