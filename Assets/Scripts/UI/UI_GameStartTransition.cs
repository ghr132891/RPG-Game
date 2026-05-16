using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_GameStartTransition : MonoBehaviour
{
    [Header("UI 引用")]
    public Image animatedImage;
    public GameObject transitionPanel;

    [Tooltip("手动把场景里的 UI_FadeScreen 拖到这里！")]
    public UI_FadeScreen fadeScreen;

    [Header("时间控制")]
    public float animationDuration = 2.0f;
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
        transitionPanel.SetActive(false);
    }

    public void PlayTransition(Action onComplete)
    {
        transitionPanel.SetActive(true);
        StartCoroutine(TransitionRoutine(onComplete));
    }

    private IEnumerator TransitionRoutine(Action onComplete)
    {
        // 1. 确保材质的消散值为 0（完全不透明）
        dissolveMaterial.SetFloat(dissolveAmountParamID, 0f);

        Animator anim = animatedImage.GetComponent<Animator>();
        if (anim != null)
        {
            anim.speed = 1f; // 确保每次开始时恢复正常播放速度
            anim.Play("StartAnim", -1, 0f);
        }

        // 2. 等待GIF播放
        yield return new WaitForSeconds(animationDuration);

        // 【核心修复 1】：定格动画！
        // 强行把动画速度设为0，防止因为动画循环导致画面在消散前闪回第一帧
        if (anim != null)
        {
            anim.speed = 0f;
        }

        // 3. 执行：从中心向四周消散
        float time = 0;
        while (time < dissolveDuration)
        {
            time += Time.deltaTime;
            dissolveMaterial.SetFloat(dissolveAmountParamID, time / dissolveDuration);
            yield return null;
        }
        // 确保完全消散，露出底部的静态图片
        dissolveMaterial.SetFloat(dissolveAmountParamID, 1f);

        // 4. 在静态图片上停留一会，让玩家欣赏
        yield return new WaitForSeconds(staticImageHoldTime);

        // 【核心修复 2】：删除自己做的 DoFadeOut，避免与 GameManager 冲突！
        // 直接调用回调函数，让 GameManager 用它原生的逻辑去接管黑屏和切场景
        onComplete?.Invoke();
    }
}