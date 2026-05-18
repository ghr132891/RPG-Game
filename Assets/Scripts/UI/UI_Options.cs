using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Options : MonoBehaviour
{

    private Player player;

    [SerializeField] private Toggle healthBarTooggle;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 20;

    [Header("BGM Volume Settings")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private string bgmParameter;

    [Header("SFX Volume Settings")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private string sfxParameter;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();

        // 加上判空保护
        if (healthBarTooggle != null)
        {
            healthBarTooggle.onValueChanged.AddListener(OnHealthBarToggleChanged);
        }

    }

    public void PlayButtonSFX()
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopBGM();
            AudioManager.instance.PlayGlobalSFX("button_Click");
        }
    }

    public void BGMSliderValue(float value)  
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(bgmParameter, newValue);
    }

    public void SFXSliderValue(float value)
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(sfxParameter, newValue);
    }
    private void OnHealthBarToggleChanged(bool isOn)
    {
        player.health.EnableHealthBar(isOn);
    }

    private void OnEnable()
    {
        sfxSlider.value = PlayerPrefs.GetFloat(sfxParameter, .6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParameter, .6f);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(sfxParameter, sfxSlider.value);
        PlayerPrefs.SetFloat(bgmParameter, bgmSlider.value);
    }

    public void LoadUpVolume()
    {
        sfxSlider.value = PlayerPrefs.GetFloat(sfxParameter, .6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParameter, .6f);
    }

    // ================= 新增：脱离卡死功能 =================
    public void UnstuckPlayer()
    {
        // 确保能获取到玩家
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (player != null && !player.health.isDead)
        {
            Debug.Log("玩家使用了脱离卡死功能！");

            // 【强烈推荐方案】：直接赋予极大伤害，走正常的死亡复活流程
            // 这样能彻底清空玩家可能卡住的物理速度和状态机，并回到上一个存档点
            player.EntityDeath();

            /* // 【备用方案】：无伤直接传送回存档点（如果你不想给死亡惩罚，可以使用这段代码并注释掉上面的受击代码）
            Vector3 safePos = GameManager.instance.GetNewPlayerPosition(RespawnType.NoneSpecific);
            if (safePos != Vector3.zero)
            {
                player.transform.position = safePos;
                player.rb.linearVelocity = Vector2.zero; // 清除可能残留的异常速度
                player.stateMachine.ChangeState(player.idleState); // 强制恢复站立
            }
            */

            // 关闭设置界面 (隐藏 Options 面板)
            gameObject.SetActive(false);

            // 如果你打开 Options 时游戏暂停了（TimeScale = 0），必须在这里恢复时间，否则死亡动画会卡住
            if (Time.timeScale == 0)
            {
                Time.timeScale = 1;
            }
        }
    }
    // ==========================================================
    public void GoMainMenuButton() => GameManager.instance.ChangeScene("MainMenu",RespawnType.NoneSpecific);
}
