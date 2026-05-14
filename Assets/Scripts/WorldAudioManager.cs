using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldAudioManager : MonoBehaviour
{
    [Header("BGM 配置 (对应 AudioDataBase 中的 audioName)")]
    public string normalBGM = "BGM_Normal";
    public string mirrorBGM = "BGM_Mirror";
    public string timeBGM = "BGM_Time";

    private void OnEnable()
    {
        // 监听场景加载事件，确保从主菜单跳转后能触发逻辑
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 如果是主菜单，保持静默，不碰同步系统
        if (scene.name == "MainMenu") return;

        // 进入关卡场景后，延迟一丁点时间执行，确保 WorldManager 已就绪
        Invoke(nameof(InitializeLevelMusic), 0.1f);
    }

    private void InitializeLevelMusic()
    {
        if (WorldManager.Instance != null && AudioManager.instance != null)
        {
            // 1. 尝试初始化（如果音乐相同，AudioManager 会自动跳过）
            AudioManager.instance.InitSyncedWorldBGM(normalBGM, mirrorBGM, timeBGM);

            // 2. 重新绑定新场景的 WorldManager 事件
            WorldManager.Instance.OnWorldChanged -= HandleWorldBGM;
            WorldManager.Instance.OnWorldChanged += HandleWorldBGM;

            // 3. 【关键】：立即根据新关卡当前的 World 状态调整音量
            // 这样即使音乐在播，也能确保它是当前世界该有的音量
            HandleWorldBGM(WorldManager.Instance.currentWorld);
        }
    }

    private void Start()
    {
        // 【新增检查】：如果当前是主菜单场景，直接退出，啥也不干
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Debug.Log("检测到主菜单场景，WorldAudioManager 保持静默。");
            return;
        }

        if (WorldManager.Instance != null && AudioManager.instance != null)
        {
            // 1. 游戏刚开始时，先将三首音乐载入底层同步系统
            AudioManager.instance.InitSyncedWorldBGM(normalBGM, mirrorBGM, timeBGM);

            WorldManager.Instance.OnWorldChanged += HandleWorldBGM;

            // 2. 初始化当前世界的音量状态
            HandleWorldBGM(WorldManager.Instance.currentWorld);
        }
    }

    private void OnDestroy()
    {
        // 场景销毁时注销事件，防止内存泄漏报错
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnWorldChanged -= HandleWorldBGM;
        }
    }

    private void HandleWorldBGM(WorldType newWorld)
    {
        if (AudioManager.instance == null) return;

        // 如果当前是在主菜单（可以根据场景名称判断），则不执行同步逻辑
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            return;
        }

        // 调用高级同步切换逻辑
        AudioManager.instance.SwitchSyncedWorldBGM(newWorld);
    }
}
