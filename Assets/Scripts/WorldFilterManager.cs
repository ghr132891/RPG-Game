using System.Collections;
using UnityEngine;
using UnityEngine.Rendering; // 用于控制 Volume
using UnityEngine.UI; // 用于 RawImage

public class WorldFilterManager : MonoBehaviour
{
    public static WorldFilterManager Instance;

    [Header("Volumes (后处理)")]
    [Tooltip("挂载了 Color Adjustments 和 Film Grain 的 Volume")]
    public Volume timeWorldVolume;
    [Tooltip("挂载了 Chromatic Aberration (色差) 的 Volume")]
    public Volume mirrorWorldVolume;

    [Header("Transition Screen (唯一全屏 UI 物体)")]
    [Tooltip("最顶层 Canvas 下全屏的 RawImage")]
    public RawImage transitionScreen;

    [Header("Materials (材质球库)")]
    [Tooltip("拖入你之前做的 RippleTransitionShader 材质球")]
    public Material mirrorMaterial;
    [Tooltip("拖入你刚替换代码的 TimeWorldNoiseShader 材质球")]
    public Material timeMaterial;

    [Header("Time World Settings (时间世界设置)")]
    [Tooltip("摄像机视口基础抖动倍率 (影响像素微震颤幅度)")]
    public float glitchIntensity = 0.03f;
    private Coroutine timeRoutine;

    [Header("Mirror World Settings (镜像世界设置)")]
    [Tooltip("镜像波纹扩散持续时间")]
    public float mirrorTransitionDuration = 0.6f;

    [Header("Audio (音效)")]
    [Tooltip("倒带/卡壳音效")]
    public AudioClip timeRewindSFX;
    [Tooltip("水波纹/色散音效")]
    public AudioClip mirrorRippleSFX;
    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();

        // 初始状态隐藏全屏特效
        if (transitionScreen != null)
        {
            transitionScreen.enabled = false;
        }
    }

    private void Start()
    {
        if (WorldManager.Instance != null)
        {
            // 订阅世界切换事件
            WorldManager.Instance.OnWorldChanged += HandleWorldTransition;
        }
    }

    private void OnDestroy()
    {
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnWorldChanged -= HandleWorldTransition;
        }
    }

    private void HandleWorldTransition(WorldType newWorld)
    {
        // 1. 先清理所有协程和特效状态
        StopAllCoroutines();
        // 每次切换时强制归位摄像机
        Camera.main.rect = new Rect(0, 0, 1, 1);
        if (timeRoutine != null) { timeRoutine = null; }

        if (timeWorldVolume != null) timeWorldVolume.weight = 0;
        if (mirrorWorldVolume != null) mirrorWorldVolume.weight = 0;
        if (transitionScreen != null) transitionScreen.enabled = false;

        // 【关键防御】：确保摄像机视口恢复正常，防止晃动到一半切世界
        Camera.main.rect = new Rect(0, 0, 1, 1);

        // 2. 根据新世界触发对应的特效协程
        switch (newWorld)
        {
            case WorldType.Time:
                PlaySFX(timeRewindSFX);
                if (timeWorldVolume != null) timeWorldVolume.weight = 1; // 瞬间变成黑白噪点
                timeRoutine = StartCoroutine(TimeWorldRoutine());
                break;

            case WorldType.Mirror:
                PlaySFX(mirrorRippleSFX);
                StartCoroutine(MirrorWorldRoutine());
                break;

            case WorldType.Normal:
                // Normal 时已在上面清理完毕
                break;
        }
    }

    // ==========================================
    // 时间世界：仅开启噪点材质，取消所有摄像机抖动
    // ==========================================
    private IEnumerator TimeWorldRoutine()
    {
        if (transitionScreen == null || timeMaterial == null) yield break;

        // 切换材质并开启 UI
        transitionScreen.material = timeMaterial;
        transitionScreen.enabled = true;

        // 确保摄像机视口是正常的 (防止之前残留的抖动)
        Camera.main.rect = new Rect(0, 0, 1, 1);

        // 这里不需要写 while 循环了，因为所有的“折线扭曲”都在 Shader 内部自动运行
        yield return null;
    }

    // ==========================================
    // 镜像世界：动态切换波纹材质 + 唤醒色差
    // ==========================================
    private IEnumerator MirrorWorldRoutine()
    {
        if (transitionScreen == null || mirrorMaterial == null) yield break;

        // 【核心操作】：动态把 UI 的材质球换成魔法波纹材质球！
        transitionScreen.material = mirrorMaterial;
        transitionScreen.enabled = true;

        // 1. 获取主角在屏幕上的 UV 坐标 (0~1 之间) 作为波纹中心
        Vector3 playerScreenPos = new Vector3(0.5f, 0.5f, 0); // 兜底：屏幕中心
        if (Player.instance != null)
        {
            playerScreenPos = Camera.main.WorldToViewportPoint(Player.instance.transform.position);
        }
        transitionScreen.material.SetVector("_Center", new Vector4(playerScreenPos.x, playerScreenPos.y, 0, 0));

        float timer = 0;
        while (timer < mirrorTransitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / mirrorTransitionDuration; // 0 到 1 的进度

            // 2. 让 UI 上的水波纹圆环不断变大 (使用 RippleTransitionShader 材质球)
            float radius = Mathf.Lerp(0f, 2.0f, progress);
            transitionScreen.material.SetFloat("_Radius", radius);

            // 3. 【核心视觉魔法】：在波纹扩大的同时，把真实的色差 Volume 慢慢淡入
            if (mirrorWorldVolume != null)
            {
                mirrorWorldVolume.weight = Mathf.Lerp(0f, 1f, progress);
            }

            yield return null;
        }

        // 过渡结束：将半径锁在最大，确保色差权重为 1（常驻）
        transitionScreen.material.SetFloat("_Radius", 2.0f);
        if (mirrorWorldVolume != null) mirrorWorldVolume.weight = 1f;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}