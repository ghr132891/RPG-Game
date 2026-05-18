using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [Header("流程引用")]
    public UI_GameStartTransition transitionController;
    public UI_StoryIntro storyController;

    private void Start()
    {
        transform.root.GetComponentInChildren<UI_Options>(true).LoadUpVolume();

        UI_FadeScreen fade = transform.root.GetComponentInChildren<UI_FadeScreen>(true);
        if (fade != null)
        {
            fade.gameObject.SetActive(true);
            fade.DoFadeIn();
        }

        AudioManager.instance.StartBGM("playList_MainMenu");
    }

    public void PlayButton()
    {
        // 【核心修复 1】：最简单暴力的隐藏法！直接把主菜单物体关掉。
        // 因为转场面板和剧情面板与它是兄弟层级，所以关掉它绝对不会影响后续动画！
        gameObject.SetActive(false);

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBGM();
            AudioManager.instance.PlayGlobalSFX("button_Click");
        }

        // 开始流水线：转场 -> 剧情 -> 呼叫 GameManager 切场景
        if (transitionController != null)
        {
            transitionController.PlayTransition(() =>
            {
                if (storyController != null)
                {
                    storyController.StartStory(() =>
                    {
                        GameManager.instance.ContinuePlay();
                    });
                }
                else
                {
                    GameManager.instance.ContinuePlay();
                }
            });
        }
        else
        {
            GameManager.instance.ContinuePlay();
        }
    }

    public void PlayButtonSFX()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBGM();
            AudioManager.instance.PlayGlobalSFX("button_Click");
        }
    }

    public void QuitGameButton()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBGM();
            AudioManager.instance.PlayGlobalSFX("button_Click");
        }
        Debug.Log("玩家点击了退出游戏！"); // 打印日志，方便你在控制台确认按钮被成功按下了

        // 如果想用纯代码控制声音，也可以在这里加一句：
        // AudioManager.instance.PlaySFX(音效ID, null);

        // 兼容编辑器的退出逻辑
#if UNITY_EDITOR
        // 如果在 Unity 编辑器中，强制停止运行模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 如果是打包后的真实游戏，执行正式退出
        Application.Quit(); 
#endif
    }
}