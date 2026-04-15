using System;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;
    [SerializeField] private GameObject levelRoot;

    public WorldType currentWorld = WorldType.Normal;

    // 定义一个事件，当世界切换时，通知所有的机关和玩家 
    public event Action<WorldType> OnWorldChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // 测试按键：按 3，4，5 切换世界
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWorld(WorldType.Normal);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchWorld(WorldType.Time);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchWorld(WorldType.Mirror);
    }



    public void SwitchWorld(WorldType newWorld)
    {
        if (currentWorld == newWorld) return;

        currentWorld = newWorld;
        Debug.Log("已切换到世界: " + currentWorld);

        // 触发事件
        OnWorldChanged?.Invoke(currentWorld);

        // 处理时间世界的特殊逻辑（简易版：暂停） 
        if (currentWorld == WorldType.Time)
        {
            Time.timeScale = 1f; // 冻结时间 
            // 注意：如果玩家在时间世界要能动，需要把玩家身上的动画/物理更新模式改为 UnscaledTime 
            Player.instance.SetTimeImmunity(true);
        }
        else if(currentWorld == WorldType.Mirror)
        {
            levelRoot.transform.localScale = new Vector3(-1, 1, 1);
            Time.timeScale = 1f; // 恢复正常
            Player.instance.SetTimeImmunity(false);
        }
        else
        {
            Time.timeScale = 1f; // 恢复正常
            Player.instance.SetTimeImmunity(false);
        }
    }
        
    }


