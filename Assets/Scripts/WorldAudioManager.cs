using UnityEngine;

public class WorldAudioManager : MonoBehaviour
{
    [Header("BGM 配置 (对应 AudioDataBase 中的 audioName)")]
    public string normalBGM = "BGM_Normal";
    public string mirrorBGM = "BGM_Mirror";
    public string timeBGM = "BGM_Time";

    private void Start()
    {
        // 订阅世界切换事件
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnWorldChanged += HandleWorldBGM;

            // 游戏刚开始时，初始化播放当前世界的 BGM
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

        // 根据切换后的世界类型，调用现有的 AudioManager 逻辑
        switch (newWorld)
        {
            case WorldType.Normal:
                AudioManager.instance.StartBGM(normalBGM);
                break;
            case WorldType.Mirror:
                AudioManager.instance.StartBGM(mirrorBGM);
                break;
            case WorldType.Time:
                AudioManager.instance.StartBGM(timeBGM);
                break;
        }
    }
}
