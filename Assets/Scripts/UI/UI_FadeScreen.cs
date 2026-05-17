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
        if (fadeImage == null) fadeImage = GetComponent<Image>();

        if (fadeImage != null)
        {
            // 【核心修复 2】：核弹级强制唤醒！
            gameObject.SetActive(true); // 激活物体
            this.enabled = true;        // 确保脚本组件没被勾掉

            // 循环强制唤醒它的所有“老爸”和“爷爷”，绝不让协程报错！
            Transform p = transform.parent;
            while (p != null)
            {
                p.gameObject.SetActive(true);
                p = p.parent;
            }

            fadeImage.color = new Color(0, 0, 0, 1);
            FadeEffect(0f, duration);
        }
    }

    public void DoFadeOut(float duration = 1)
    {
        if (fadeImage == null) fadeImage = GetComponent<Image>();

        if (fadeImage != null)
        {
            // 【核心修复 2】：核弹级强制唤醒！
            gameObject.SetActive(true);
            this.enabled = true;

            Transform p = transform.parent;
            while (p != null)
            {
                p.gameObject.SetActive(true);
                p = p.parent;
            }

            fadeImage.color = new Color(0, 0, 0, 0);
            FadeEffect(1f, duration);
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
