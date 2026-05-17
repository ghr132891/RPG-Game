using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_GameStartTransition : MonoBehaviour
{
    [Header("UI 引用")]
    public Image animatedImage;
    public GameObject transitionPanel;

    [Header("时间控制")]
    [Tooltip("点击开始后，继续播放动态图几秒")]
    public float delayBeforeDissolve = 2.0f;
    public float dissolveDuration = 1.5f;
    public float staticImageHoldTime = 1.0f;

    private Material dissolveMaterial;
    private int dissolveAmountParamID = Shader.PropertyToID("_DissolveAmount");

    private void Awake()
    {
        if (animatedImage != null)
        {
            dissolveMaterial = new Material(animatedImage.material);
            animatedImage.material = dissolveMaterial;
        }

        // 【核心修改】：现在它作为背景，一开始就要显示，千万不能隐藏！
        transitionPanel.SetActive(true);
    }

    public void PlayTransition(Action onComplete)
    {
        StartCoroutine(TransitionRoutine(onComplete));
    }

    private IEnumerator TransitionRoutine(Action onComplete)
    {
        // 1. 确保材质未消散
        dissolveMaterial.SetFloat(dissolveAmountParamID, 0f);

        // 2. 此时动画已经在循环播放了，我们只需要等待指定的几秒钟
        yield return new WaitForSeconds(delayBeforeDissolve);

        // 3. 准备溶解前，把动画定格，防止消散到一半它又循环回第一帧
        Animator anim = animatedImage.GetComponent<Animator>();
        if (anim != null) anim.speed = 0f;

        // 4. 执行：从中心向四周消散
        float time = 0;
        while (time < dissolveDuration)
        {
            time += Time.deltaTime;
            dissolveMaterial.SetFloat(dissolveAmountParamID, time / dissolveDuration);
            yield return null;
        }
        dissolveMaterial.SetFloat(dissolveAmountParamID, 1f);

        // 5. 静态图停留一会
        yield return new WaitForSeconds(staticImageHoldTime);

        // 6. 转场表演结束，呼叫主菜单进入下一步（播剧情）
        onComplete?.Invoke();
    }
}