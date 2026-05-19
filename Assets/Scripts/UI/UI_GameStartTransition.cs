using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_GameStartTransition : MonoBehaviour
{
    [Header("UI 引用")]
    public Image animatedImage;
    [Tooltip("放在 animatedImage 底下/后面的静态背景图")]
    public Image staticImage;
    public GameObject transitionPanel;

    [Header("时间控制")]
    [Tooltip("点击开始后，继续播放动态图几秒")]
    public float delayBeforeDissolve = 2.0f;
    [Tooltip("火焰燃烧扩散的持续时间")]
    public float dissolveDuration = 3.5f;
    [Tooltip("完全露出静态图后的停留时间")]
    public float staticImageHoldTime = 1.0f;

    [Header("进度区间控制 (核心修改)")]
    [Range(0f, 1f)]
    [Tooltip("燃烧开始时的初始进度，填0.1~0.2可以跳过起步阶段")]
    public float startProgress = 0.1f;  // 不从0开始
    [Range(0f, 1f)]
    [Tooltip("燃烧结束时的目标进度，填0.7左右即可")]
    public float endProgress = 0.7f;    // 在0.7结束

    [Header("其他组件")]
    [Tooltip("如果你加了灰烬粒子，把它拖进这里")]
    public ParticleSystem ashParticles;

    private Material dissolveMaterial;
<<<<<<< Updated upstream
    private Material originalMaterial; // 【新增】：用于记录原始材质
=======
>>>>>>> Stashed changes
    private int progressParamID = Shader.PropertyToID("_Progress");

    private void Awake()
    {
        if (transitionPanel != null) transitionPanel.SetActive(true);

        if (animatedImage != null && animatedImage.material != null)
        {

            // 【核心修改】：先备份原版材质！
            originalMaterial = animatedImage.material;
            dissolveMaterial = new Material(animatedImage.material);
            // 游戏一开始，直接把材质的进度设为起始值
            dissolveMaterial.SetFloat(progressParamID, startProgress);
            animatedImage.material = dissolveMaterial;
        }
    }

    public void PlayTransition(Action onComplete)
    {
        StartCoroutine(TransitionRoutine(onComplete));
    }

    private IEnumerator TransitionRoutine(Action onComplete)
    {
        if (staticImage != null) staticImage.gameObject.SetActive(true);
        if (animatedImage != null) animatedImage.gameObject.SetActive(true);
        if (dissolveMaterial != null) dissolveMaterial.SetFloat(progressParamID, startProgress);

        yield return new WaitForSeconds(delayBeforeDissolve);

        Animator anim = animatedImage.GetComponent<Animator>();
        if (anim != null) anim.speed = 0f;

        // 如果配置了粒子系统，在这里触发飞灰
        if (ashParticles != null) ashParticles.Play();

        float time = 0;
        while (time < dissolveDuration)
        {
            time += Time.deltaTime;

            // 计算 0 到 1 的标准化时间
            float normalizedProgress = time / dissolveDuration;

            // 【核心算法】：把 0~1 的时间，平滑映射到 startProgress ~ endProgress 之间
            float curveProgress = Mathf.SmoothStep(startProgress, endProgress, normalizedProgress);

            if (dissolveMaterial != null) dissolveMaterial.SetFloat(progressParamID, curveProgress);
            yield return null;
        }

        // 确保最终定格在目标结束值
        if (dissolveMaterial != null) dissolveMaterial.SetFloat(progressParamID, endProgress);

        // 注意：如果你结束在0.7时，屏幕上还有没烧完的火焰边缘
        // 最好【不要】立刻关掉图片，否则画面会生硬闪烁消失
        // 我注释掉了下面这行代码，让火焰边缘能完美驻留到转场彻底结束！
        // if (animatedImage != null) animatedImage.gameObject.SetActive(false); 

        yield return new WaitForSeconds(staticImageHoldTime);

        // 呼叫主菜单进入下一步（播剧情/进游戏）
        onComplete?.Invoke();
    }

    private void OnDestroy()
    {
<<<<<<< Updated upstream
        // 【核心修复】：先把原始材质还给图片，防止 Unity 序列化检查报错
        if (animatedImage != null && originalMaterial != null)
        {
            animatedImage.material = originalMaterial;
        }
        if (dissolveMaterial != null)
            Destroy(dissolveMaterial);
=======
        if (dissolveMaterial != null) Destroy(dissolveMaterial);
>>>>>>> Stashed changes
    }
}