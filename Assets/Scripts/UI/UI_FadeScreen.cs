using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_FadeScreen : MonoBehaviour
{
    private Image fadeImage;
    public Coroutine fadeEffectCo { get; private set; }

    private void Awake()
    {
        fadeImage = GetComponent<Image>();
        fadeImage.color = new Color(0,0,0,1);
    }

    public void DoFadeIn(float duration = 1)
    {
        // 1. 安全检查：如果还未获取到 Image，立刻获取一次
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }

        // 2. 再次检查，防止真的没挂载 Image 组件导致报错
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 1);
            FadeEffect(0f, duration);
        }
        else
        {
            Debug.LogError("UI_FadeScreen 找不到 Image 组件！请检查它所在的物体。");
        }
    }

    public void DoFadeOut(float duration = 1)
    {
        // 1. 安全检查：如果还未获取到 Image，立刻获取一次
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            FadeEffect(1f, duration);
        }
        else
        {
            Debug.LogError("UI_FadeScreen 找不到 Image 组件！请检查它所在的物体。");
        }
    }

    private void FadeEffect(float targetAlpha,float duration)
    {
        if (fadeEffectCo != null)
            StopCoroutine(fadeEffectCo);

        fadeEffectCo = StartCoroutine(FadeEffectCo(targetAlpha, duration));
    }

    private IEnumerator FadeEffectCo(float targetAlpha,float duration)
    {
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            var color = fadeImage.color;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            fadeImage.color = color;

            yield return null;
        }

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
    }
    

    

}
