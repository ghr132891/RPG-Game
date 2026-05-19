using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour, ISaveable
{
    public static GameManager instance;
    private Vector3 lastPlayerPosition;
    public string lastScenePlayed;
    private bool dataLoaded;

    [HideInInspector] public string lastActivatedCheckPointID;

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

    public void ContinuePlay()
    {
        if (string.IsNullOrEmpty(lastScenePlayed))
            lastScenePlayed = "Level_Sleeping Mine_1";
        if (SaveManager.instance.GetGameData() == null)
            lastScenePlayed = "Level_Sleeping Mine_1";

        ChangeScene(lastScenePlayed, RespawnType.NoneSpecific);
    }

    public void RestartScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        ChangeScene(sceneName, RespawnType.NoneSpecific);
    }

    public void ChangeScene(string sceneName, RespawnType respawnType)
    {
        // 在保存游戏之前，如果发现玩家是死的，立刻强行救活并重置状态！
        if (Player.instance != null && Player.instance.health.isDead)
        {
            // 【核心修复 1】：强行把玩家从 DeadState 拉回到 IdleState，防止卡死
            Player.instance.stateMachine.ChangeState(Player.instance.idleState);
            Player.instance.health.Revive();
        }

        if (Player.instance != null)
        {
            // 黑屏过渡期间，强行剥夺玩家的受伤判定，防止被机关二次鞭尸
            Player.instance.health.SetCanTakeDamage(false);

            // 【核心修复 2】：清空物理惯性！防止卡墙死时的巨大异常受力带到复活点
            Player.instance.rb.linearVelocity = Vector2.zero;
        }

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

        dataLoaded = false;
        yield return null;

        while (dataLoaded == false)
        {
            yield return null;
        }

        // ==========================================
        // 【核心修复 3】：多等待一点真实时间！(0.1秒)
        // 之前的 WaitForEndOfFrame 太短了！导致天赋/装备的 MaxHealth 还没加到属性面板上。
        // 必须让所有组件的 Start() 和 LoadData() 彻底跑完，再执行奶满！
        // ==========================================
        yield return new WaitForSeconds(0.1f);

        fadeScreen = FindFadeScreenUI();
        fadeScreen.DoFadeIn(); // black > transperent

        Player player = Player.instance;

        if (player == null)
            yield break;

        // 再次加固：此时装备属性已彻底生效，这口奶绝对能奶到“装备加成后的真正满血”
        player.stateMachine.ChangeState(player.idleState);
        player.rb.linearVelocity = Vector2.zero;
        player.health.Revive();

        // 恢复正常受伤判定
        player.health.SetCanTakeDamage(true);

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
        if (type == RespawnType.Portal)
        {
            Object_Portal portal = Object_Portal.instance;

            Vector3 position = portal.GetPosition();

            portal.SetTrigger(false);
            portal.DisableIfNeeded();

            return position;
        }

        if (type == RespawnType.NoneSpecific)
        {
            var data = SaveManager.instance.GetGameData();
            var checkPoints = FindObjectsByType<Object_CheckPoint>(FindObjectsSortMode.None);

            // ===============================================
            // 优先级 1. 优先寻找玩家生前最后一次触碰的存档点
            // ===============================================
            if (!string.IsNullOrEmpty(lastActivatedCheckPointID))
            {
                Object_CheckPoint lastPoint = checkPoints.FirstOrDefault(cp => cp.GetCheckPointID() == lastActivatedCheckPointID);

                if (lastPoint != null && data.unlockedCheckPoints.TryGetValue(lastPoint.GetCheckPointID(), out bool isUnlocked) && isUnlocked)
                {
                    Debug.Log("复活成功：找到了最后激活的检查点！");
                    return lastPoint.GetPosition();
                }
            }

            // ===============================================
            // 优先级 2. 【修复核心】如果由于读档等原因丢了ID，
            // 寻找离玩家死前最近且已经解锁的存档点！
            // ===============================================
            var unlockedCheckPoints = checkPoints
                .Where(cp => data.unlockedCheckPoints.TryGetValue(cp.GetCheckPointID(), out bool unlocked) && unlocked)
                .Select(cp => cp.GetPosition())
                .ToList();

            if (unlockedCheckPoints.Count > 0)
            {
                Debug.Log("复活成功：通过就近原则，找到了离玩家最近的已解锁检查点！");
                return unlockedCheckPoints.OrderBy(position => Vector3.Distance(position, lastPlayerPosition)).First();
            }

            // ===============================================
            // 优先级 3. 终极兜底：完全没有解锁任何存档点，才去场景入口
            // ===============================================
            var enterWayPoints = FindObjectsByType<Object_WayPoint>(FindObjectsSortMode.None)
                .Where(wp => wp.GetWayPointType() == RespawnType.Enter)
                .Select(wp => wp.GetPositionAndSetTriggerFalse())
                .ToList();

            if (enterWayPoints.Count > 0)
            {
                Debug.Log("复活兜底：没有找到可用检查点，回到场景入口！");
                return enterWayPoints.OrderBy(p => Vector3.Distance(p, lastPlayerPosition)).First();
            }

            return Vector3.zero;
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
        lastActivatedCheckPointID = gameData.lastCheckPointID;

        if (string.IsNullOrEmpty(lastScenePlayed))
            lastScenePlayed = "Level_Sleeping Mine_1";

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
        gameData.lastCheckPointID = lastActivatedCheckPointID;

        dataLoaded = false;
    }
}