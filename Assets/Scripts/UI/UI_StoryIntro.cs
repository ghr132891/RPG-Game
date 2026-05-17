using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UI_StoryIntro : MonoBehaviour
{
    [Header("UI 引用")]
    public Image backgroundImage;
    public TextMeshProUGUI textDisplay;

    [Header("背景设置")]
    public Color bgColor = Color.black;
    [Tooltip("背景颜色从透明到完全显示的渐变耗时")]
    public float bgFadeDuration = 1.5f; // 【新增】：背景淡入时间

    [Header("剧情文字")]
    [TextArea(3, 10)]
    public string[] storySegments;

    [Header("设置")]
    public float typingSpeed = 0.05f;

    private int currentSegmentIndex = 0;
    private bool isTyping = false;
    private bool segmentComplete = false;
    private bool isFadingBg = false; // 【新增】：标记是否正在淡入背景

    private Coroutine typingCoroutine;
    private Coroutine fadeCoroutine; // 【新增】：记录淡入协程
    private Action onStoryEnd;

    void Awake()
    {
        // 【核心修改】：初始化时，强制把背景颜色的透明度设为 0 (完全透明)
        if (backgroundImage != null)
        {
            Color startColor = bgColor;
            startColor.a = 0f;
            backgroundImage.color = startColor;
        }
        textDisplay.text = "";
        gameObject.SetActive(false);
    }

    public void StartStory(Action onComplete)
    {
        gameObject.SetActive(true);
        onStoryEnd = onComplete;
        currentSegmentIndex = 0;

        // 【核心修改】：不直接打字，而是先启动背景淡入协程
        fadeCoroutine = StartCoroutine(FadeInAndStart());
    }

    // 【新增】：背景逐渐浮现的过渡协程
    private IEnumerator FadeInAndStart()
    {
        isFadingBg = true;
        if (backgroundImage != null)
        {
            float time = 0;
            while (time < bgFadeDuration)
            {
                time += Time.deltaTime;
                Color c = bgColor;
                // 透明度从 0 逐渐过渡到设定的 Alpha 值 (通常是1)
                c.a = Mathf.Lerp(0f, bgColor.a, time / bgFadeDuration);
                backgroundImage.color = c;
                yield return null;
            }
            backgroundImage.color = bgColor; // 确保最终颜色准确
        }

        isFadingBg = false;
        ShowNextSegment(); // 背景变黑后，再开始出字！
    }

    public void OnScreenClick()
    {
        // 【新增体验优化】：如果玩家在背景渐变时就心急点击了，直接跳过渐变，瞬间变黑并开始打字
        if (isFadingBg)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            if (backgroundImage != null) backgroundImage.color = bgColor;
            isFadingBg = false;
            ShowNextSegment();
            return;
        }

        if (isTyping)
        {
            // 正在打字时点击：直接显示本段全部文字
            StopCoroutine(typingCoroutine);
            textDisplay.text = storySegments[currentSegmentIndex];
            isTyping = false;
            segmentComplete = true;
        }
        else if (segmentComplete)
        {
            // 本段打完后点击：进入下一段
            currentSegmentIndex++;
            if (currentSegmentIndex < storySegments.Length)
            {
                ShowNextSegment();
            }
            else
            {
                EndStory();
            }
        }
    }

    private void ShowNextSegment()
    {
        segmentComplete = false;
        typingCoroutine = StartCoroutine(TypeText(storySegments[currentSegmentIndex]));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        textDisplay.text = "";

        foreach (char letter in text.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        segmentComplete = true;
    }

    private void EndStory()
    {
        // 留着纯色背景当遮挡板，等待 GameManager 的黑屏接管
        textDisplay.text = "";
        onStoryEnd?.Invoke();
    }
}