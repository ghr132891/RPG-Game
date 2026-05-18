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

    public void GoMainMenuButton() => GameManager.instance.ChangeScene("MainMenu",RespawnType.NoneSpecific);
}
