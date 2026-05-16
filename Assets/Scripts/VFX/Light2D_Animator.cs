using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class Light2D_Animator : MonoBehaviour
{
    public enum LightAnimMode { None, Pulse, Flicker, FadeOut }

    [Header("光源模式")]
    public LightAnimMode mode = LightAnimMode.None;

    [Header("通用设置")]
    public float baseIntensity = 1.0f;
    public float speed = 2.0f;

    [Header("Pulse (呼吸) / Flicker (闪烁) 设置")]
    public float amplitude = 0.5f; // 亮度浮动范围

    [Header("FadeOut (衰减销毁) 设置")]
    public float fadeDuration = 0.3f; // 多久后熄灭并销毁
    private float fadeTimer;

    private Light2D myLight;
    private float randomOffset;

    private void Awake()
    {
        myLight = GetComponent<Light2D>();
        randomOffset = Random.Range(0f, 100f); // 打乱同屏多个光的节奏
        fadeTimer = fadeDuration;

        if (mode == LightAnimMode.FadeOut)
        {
            myLight.intensity = baseIntensity; // 初始爆发高亮
        }
    }

    private void Update()
    {
        if (myLight == null) return;

        switch (mode)
        {
            case LightAnimMode.Pulse:
                // 使用正弦波产生平滑的呼吸感 (适合魔法水晶)
                float sine = Mathf.Sin(Time.time * speed + randomOffset);
                myLight.intensity = baseIntensity + sine * amplitude;
                break;

            case LightAnimMode.Flicker:
                // 使用柏林噪声产生不规则的随机闪动 (适合火把)
                float noise = Mathf.PerlinNoise(randomOffset, Time.time * speed);
                myLight.intensity = baseIntensity + (noise - 0.5f) * amplitude * 2f;
                break;

            case LightAnimMode.FadeOut:
                // 瞬间高亮，然后快速衰减并销毁 (适合刀剑碰撞、爆炸特效)
                fadeTimer -= Time.deltaTime;
                float normalizedTime = fadeTimer / fadeDuration; // 1 到 0

                // 使用平滑曲线让衰减更好看
                myLight.intensity = Mathf.Lerp(0, baseIntensity, normalizedTime * normalizedTime);

                if (fadeTimer <= 0)
                {
                    Destroy(gameObject);
                }
                break;

            case LightAnimMode.None:
            default:
                break;
        }
    }
}