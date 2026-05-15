using System;
using UnityEngine;
using System.Collections;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    [Header("世界切换设置")]
    [SerializeField] private float switchCooldown = 1f;
    private float lastSwitchTime = -Mathf.Infinity;

    [Header("时间世界设置")]
    [Range(0.01f, 2f)]
    public float timeWorldScale = 0.3f;

    [Header("滤镜渐变时间")]
    public float filterFadeDuration = 0.25f;

    public WorldType currentWorld = WorldType.Normal;
    public bool isMirrored { get; private set; }

    // 【新增】：全局时停标志位
    public bool isTimeStopped { get; private set; }

    public event Action<WorldType> OnWorldChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha3)) TrySwitchWorld(WorldType.Normal);
        //if (Input.GetKeyDown(KeyCode.Alpha4)) TrySwitchWorld(WorldType.Time);
        //if (Input.GetKeyDown(KeyCode.Alpha5)) TrySwitchWorld(WorldType.Mirror);

        // 【新增】：按下 T 键触发时停开关
        if (Input.GetKeyDown(KeyCode.T)) TryToggleTimeStop();
    }

    private WorldFilterManager FindFilterScreenUI()
    {
        return FindFirstObjectByType<WorldFilterManager>();
    }

    private void TryToggleTimeStop()
    {
        // 只有在时间世界里，才能使用时停功能
        if (currentWorld == WorldType.Time)
        {
            isTimeStopped = !isTimeStopped;
            Debug.Log(isTimeStopped ? "时停已开启：万物静止！" : "时停已解除：时间继续流动！");
        }
    }

    private void TrySwitchWorld(WorldType inputType)
    {
        if (Time.unscaledTime >= lastSwitchTime + switchCooldown)
        {
            SwitchWorld(inputType);
        }
        else
        {
            float remainingTime = (lastSwitchTime + switchCooldown) - Time.unscaledTime;
            Debug.Log($"世界切换冷却中: {remainingTime:F1}秒");
        }
    }

    public void SwitchWorld(WorldType inputType)
    {
        lastSwitchTime = Time.unscaledTime;
        isTimeStopped = false;

        StartCoroutine(SwitchWorldCo(inputType));
    }

    private IEnumerator SwitchWorldCo(WorldType inputType)
    {
        WorldFilterManager filterScreen = FindFilterScreenUI();

        /*
        // --- 视觉表现层：滤镜控制 ---
        if (filterScreen != null)
        {
            // 1. 处理退出逻辑：如果当前是时间/镜像，但新状态不是，则淡出
            if (currentWorld == WorldType.Time && inputType != WorldType.Time)
                filterScreen.DoFilterFadeOut(filterFadeDuration);
            else if (currentWorld == WorldType.Mirror && inputType != WorldType.Mirror)
                filterScreen.DoFilterFadeOut(filterFadeDuration);

            // 2. 处理进入逻辑
            if (inputType == WorldType.Time && currentWorld != WorldType.Time)
                filterScreen.DoTimeFilterFadeIn(0.35f, filterFadeDuration);
            else if (inputType == WorldType.Mirror && currentWorld != WorldType.Mirror)
                filterScreen.DoMirrorFilterFadeIn(0.35f, filterFadeDuration);

            // 3. 处理镜像和时间叠加的特殊视觉（可选）
            // 如果你的逻辑允许同时是镜像+时间，你可以在这里叠加颜色或选择混合色
        }
        */

        // --- 核心逻辑层：状态修改 (保持你原有的逻辑) ---
        lastSwitchTime = Time.unscaledTime;
        isTimeStopped = false;

        if (inputType == WorldType.Normal)
        {
            SetNormalWorld();
        }
        else if (inputType == WorldType.Mirror)
        {
            if (currentWorld == WorldType.Mirror)
                SetNormalWorld();
            else
            {
                ResetTimeScale(); // 确保镜像界初始时间正常
                isMirrored = true;
                currentWorld = WorldType.Mirror;
            }
        }
        else if (inputType == WorldType.Time)
        {
            if (currentWorld == WorldType.Time)
            {
                ResetTimeScale();
                currentWorld = isMirrored ? WorldType.Mirror : WorldType.Normal;
            }
            else
            {
                ApplyTimeScale(); // 进入减速状态
                currentWorld = WorldType.Time;
            }
        }

        OnWorldChanged?.Invoke(currentWorld);
        yield return null;
    }

    // 辅助方法：重置时间
    private void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (Player.instance != null) Player.instance.SetTimeImmunity(false);
    }

    // 辅助方法：应用时间减速
    private void ApplyTimeScale()
    {
        Time.timeScale = timeWorldScale;
        Time.fixedDeltaTime = 0.02f * timeWorldScale;
        if (Player.instance != null) Player.instance.SetTimeImmunity(true);
    }

    private void SetNormalWorld()
    {
        isMirrored = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (Player.instance != null) Player.instance.SetTimeImmunity(false);
        currentWorld = WorldType.Normal;
    }
}