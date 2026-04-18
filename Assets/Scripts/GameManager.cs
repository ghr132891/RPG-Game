using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour,ISaveable
{
    public static GameManager instance;
    private Vector3 lastPlayerPosition;
    public string lastScenePlayed;
    private bool dataLoaded;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    //public void SetLastPlayerPosition(Vector3 position) => lastPlayerPosition = position;

    public void ContinuePlay()
    {
        if (string.IsNullOrEmpty(lastScenePlayed))
            lastScenePlayed = "Level_0";
        if (SaveManager.instance.GetGameData() == null)
            lastScenePlayed = "Level_0";
            
        ChangeScene(lastScenePlayed, RespawnType.NoneSpecific);
    }

    public void RestartScene()
    {

        string sceneName = SceneManager.GetActiveScene().name;
        ChangeScene(sceneName, RespawnType.NoneSpecific);
    }

    public void ChangeScene(string sceneName, RespawnType respawnType)
    {
        SaveManager.instance.SaveGame();

        Time.timeScale = 1;
        StartCoroutine(ChangeScnenCo(sceneName, respawnType));
    }
    private IEnumerator ChangeScnenCo(string sceneName, RespawnType respawnType)
    {
        UI_FadeScreen fadeScreen = FindFadeScreenUI();

        fadeScreen.DoFadeOut(); // transperent > black
        yield return fadeScreen.fadeEffectCo;

        SceneManager.LoadScene(sceneName);

        dataLoaded = false;// data loaded becomes true when you load game from save manager
        yield return null;

        while(dataLoaded == false)
        {
            yield return null;
        }

        fadeScreen = FindFadeScreenUI();
        fadeScreen.DoFadeIn(); // black > transperent

        Player player = Player.instance;

        if (player == null)
            yield break;

        Vector3 position = GetNewPlayerPosition(respawnType);

        if (position != Vector3.zero)
            player.TeleportPlayer(position);
    }

    private UI_FadeScreen FindFadeScreenUI()
    {
        if (UI.instance != null)
            return UI.instance.fadeScreenUI;
        else
            return FindFirstObjectByType<UI_FadeScreen>();
    }

    private Vector3 GetNewPlayerPosition(RespawnType type)
    {
        if(type == RespawnType.Portal)
        {
            Object_Portal portal = Object_Portal.instance;

            Vector3 position = portal.GetPosition();

            portal.SetTrigger(false);
            portal.DisableIfNeeded();

            return position;
        }

        if(type == RespawnType.NoneSpecific)
        {
            var data = SaveManager.instance.GetGameData();
            var checkPoints = FindObjectsByType<Object_CheckPoint>(FindObjectsSortMode.None);
            var unlockedCheckPoints = checkPoints
                .Where(cp => data.unlockedCheckPoints.TryGetValue(cp.GetCheckPointID(), out bool unlocked) && unlocked)
                .Select(cp => cp.GetPosition())
                .ToList();

            // add enterWayPoints ,choose the best way when player die in the middle of checkPoint and enterWayPoints.
            var enterWayPoints = FindObjectsByType<Object_WayPoint>(FindObjectsSortMode.None)
                .Where(wp => wp.GetWayPointType() == RespawnType.Enter)
                .Select(wp => wp.GetPositionAndSetTriggerFalse())
                .ToList();

            var selectedPosition = unlockedCheckPoints.Concat(enterWayPoints).ToList(); // combine two lists into one.

            if (selectedPosition.Count == 0)
                return Vector3.zero;

            return selectedPosition.OrderBy(position => Vector3.Distance(position, lastPlayerPosition)).First(); // arrange from lowest to highest.

        }

        return GetWayPointPosition(type);

    }

    private Vector3 GetWayPointPosition(RespawnType type)
    {
        var wayPoints = FindObjectsByType<Object_WayPoint>(FindObjectsSortMode.None);

        foreach (var point in wayPoints)
        {
            if (point.GetWayPointType() == type)              
                return point.GetPositionAndSetTriggerFalse();
        }
        return Vector3.zero;
    }

    public void LoadData(GameData gameData)
    {
        lastScenePlayed = gameData.lastScenePlayed;
        lastPlayerPosition = gameData.lastPlayerPosition;

        if (string.IsNullOrEmpty(lastScenePlayed))
            lastScenePlayed = "Level_0";

        dataLoaded = true;
    }

    public void SaveData(ref GameData gameData)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "MainMenu")
            return;

        Vector3 posToSave = Player.instance.transform.position;

        if (WorldManager.Instance != null && WorldManager.Instance.currentWorld == WorldType.Mirror)
        {
            posToSave.x = -posToSave.x;
        }

        gameData.lastPlayerPosition = posToSave;
        gameData.lastScenePlayed = currentScene;

        dataLoaded = false;
    }
}
