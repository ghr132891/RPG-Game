using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{


    private void Start()
    {

        transform.root.GetComponentInChildren<UI_Options>(true).LoadUpVolume();
        transform.root.GetComponentInChildren<UI_FadeScreen>().DoFadeIn();
        AudioManager.instance.StartBGM("playList_MainMenu");
    }
    public void PlayButton()
    {
        // 1. Õ£÷π÷˜≤Àµ•±≥æ∞“Ù¿÷
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBGM();
        }
        AudioManager.instance.PlayGlobalSFX("button_Click");
        GameManager.instance.ContinuePlay();
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }

}
