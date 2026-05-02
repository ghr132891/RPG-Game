using UnityEngine;

public class UI_MainMenu : MonoBehaviour
{
    private void Start()
    {
        transform.root.GetComponentInChildren<UI_FadeScreen>().DoFadeIn();
        AudioManager.instance.StartBGM("playList_MainMenu");
    }
    public void PlayButton()
    {
        AudioManager.instance.PlayGlobalSFX("button_Click");
        GameManager.instance.ContinuePlay();
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }

}
