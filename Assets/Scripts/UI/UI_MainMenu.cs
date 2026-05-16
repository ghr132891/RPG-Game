using UnityEngine;
using UnityEngine.UI; // 引入 UI 命名空间

public class UI_MainMenu : MonoBehaviour
{
    [Header("转场控制器")]
    public UI_GameStartTransition transitionController; // 在面板里把刚才挂脚本的物体拖进来

    private void Start()
    {
        transform.root.GetComponentInChildren<UI_Options>(true).LoadUpVolume();
        transform.root.GetComponentInChildren<UI_FadeScreen>().DoFadeIn();
        AudioManager.instance.StartBGM("playList_MainMenu");
    }

    public void PlayButton()
    {
        // 1. 禁用所有按钮交互，防止玩家狂点报错
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.interactable = false;

        // 2. 停止主菜单背景音乐和播放音效
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBGM();
            AudioManager.instance.PlayGlobalSFX("button_Click");

            // 你甚至可以在这里播放一声极具气势的转场音效
            // AudioManager.instance.PlayGlobalSFX("GameStart_Transition"); 
        }

        // 3. 开始优雅的转场表演
        if (transitionController != null)
        {
            // 使用 Lambda 表达式，当转场结束时，自动执行大括号里的逻辑
            transitionController.PlayTransition(() =>
            {
                GameManager.instance.ContinuePlay(); // 表演结束，加载游戏场景！
            });
        }
        else
        {
            // 防呆设计：如果没配置转场控制器，直接加载游戏
            GameManager.instance.ContinuePlay();
        }
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }
}