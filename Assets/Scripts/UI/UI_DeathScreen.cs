using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_DeathScreen : MonoBehaviour
{


    public void GoToCheckPointButton()
    {
        GameManager.instance.RestartScene();
    }

    public void GoToMainMenu()
    {
        GameManager.instance.ChangeScene("MainMenu", RespawnType.NoneSpecific);
    }

}
