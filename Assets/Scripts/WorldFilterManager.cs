using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_WorldFilterScreen : MonoBehaviour
{
    private Image filterImage;
    public Coroutine filterEffectCo { get; private set; }

    [Header("滤镜颜色设置")]
    public Color timeWorldColor = new Color(0.1f, 0.4f, 0.9f, 1f);   // 蓝色
    public Color mirrorWorldColor = new Color(0.6f, 0.2f, 0.8f, 1f); // 紫色

    private void Awake()
    {
        filterImage = GetComponent<Image>();
        // 初始全透明
        filterImage.color = new Color(1, 1, 1, 0f);
        filterImage.raycastTarget = false;
    }

    // 进入时间世界滤镜
    public void DoTimeFilterFadeIn(float targetAlpha = 0.3f, float duration = 0.5f)
    {
        FadeEffect(timeWorldColor, targetAlpha, duration);
    }

    // 进入镜像世界滤镜
    public void DoMirrorFilterFadeIn(float targetAlpha = 0.3f, float duration = 0.5f)
    {
        FadeEffect(mirrorWorldColor, targetAlpha, duration);
    }

    // 关闭所有滤镜
    public void DoFilterFadeOut(float duration = 0.5f)
    {
        // 保持当前颜色，只把透明度转为0
        Color targetColor = new Color(filterImage.color.r, filterImage.color.g, filterImage.color.b, 0f);
        FadeEffect(targetColor, 0f, duration);
    }

    private void FadeEffect(Color targetColor, float targetAlpha, float duration)
    {
        if (filterEffectCo != null)
            StopCoroutine(filterEffectCo);

        filterEffectCo = StartCoroutine(FadeEffectCo(targetColor, targetAlpha, duration));
    }

    private IEnumerator FadeEffectCo(Color targetColor, float targetAlpha, float duration)
    {
        Color startColor = filterImage.color;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime; // 忽略时间减速影响

            // 同时插值颜色和透明度
            float lerpAlpha = Mathf.Lerp(startColor.a, targetAlpha, time / duration);
            filterImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, lerpAlpha);

            yield return null;
        }

        filterImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, targetAlpha);
    }
}