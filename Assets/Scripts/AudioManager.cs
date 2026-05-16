using System.Collections;
using UnityEngine;
using UnityEngine.Audio; // 【核心新增】：必须引入音频混音命名空间

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDataBaseSo audioDataBase;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource sfxSource;

    // ==========================================
    // 【核心新增】用于接收混音轨道分组的槽位
    // ==========================================
    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup bgmGroup; // 拖入你的 BGM 混音组
    [SerializeField] private AudioMixerGroup sfxGroup; // 拖入你的 SFX 混音组

    // 世界同步 BGM 专属音源与变量
    private AudioSource normalBgmSource;
    private AudioSource mirrorBgmSource;
    private AudioSource timeBgmSource;

    private float maxNormalVol, maxMirrorVol, maxTimeVol;
    private Coroutine syncCrossfadeCo;

    private AudioClip lastMusicPlayed;
    private string currentAudioGroupName;
    private Coroutine currentAudioCo;
    [SerializeField] private bool audioShouldPlay;

    private Transform player;

    private string currentNormalName;
    private string currentMirrorName;
    private string currentTimeName;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // ========================================================
        // 【核心修复 1】：动态创建音源时，强行将其输出指向 BGM 混音轨道
        // ========================================================
        normalBgmSource = gameObject.AddComponent<AudioSource>();
        normalBgmSource.loop = true;
        if (bgmGroup != null) normalBgmSource.outputAudioMixerGroup = bgmGroup;

        mirrorBgmSource = gameObject.AddComponent<AudioSource>();
        mirrorBgmSource.loop = true;
        if (bgmGroup != null) mirrorBgmSource.outputAudioMixerGroup = bgmGroup;

        timeBgmSource = gameObject.AddComponent<AudioSource>();
        timeBgmSource.loop = true;
        if (bgmGroup != null) timeBgmSource.outputAudioMixerGroup = bgmGroup;

        // 顺便把原本自带的主 BGM 音源也绑上轨道
        if (audioSource != null && bgmGroup != null) audioSource.outputAudioMixerGroup = bgmGroup;
        if (sfxSource != null && sfxGroup != null) sfxSource.outputAudioMixerGroup = sfxGroup;
    }

    private void Update()
    {
        if (audioSource.isPlaying == false && audioShouldPlay)
        {
            if (string.IsNullOrEmpty(currentAudioGroupName) == false)
                NextBGM(currentAudioGroupName);
        }

        if (audioSource.isPlaying && audioShouldPlay == false)
            StopBGM();
    }

    public void InitSyncedWorldBGM(string normalBGM, string mirrorBGM, string timeBGM)
    {
        if (normalBGM == currentNormalName && mirrorBGM == currentMirrorName && timeBGM == currentTimeName)
        {
            Debug.Log("检测到相同音乐，保持无缝持续播放。");
            return;
        }

        currentNormalName = normalBGM;
        currentMirrorName = mirrorBGM;
        currentTimeName = timeBGM;

        audioShouldPlay = false;
        currentAudioGroupName = null;

        if (audioSource.isPlaying)
            audioSource.Stop();

        if (currentAudioCo != null)
            StopCoroutine(currentAudioCo);

        var normalData = audioDataBase.Get(normalBGM);
        var mirrorData = audioDataBase.Get(mirrorBGM);
        var timeData = audioDataBase.Get(timeBGM);

        if (normalData != null && normalData.clips.Count > 0)
        {
            normalBgmSource.clip = normalData.clips[0];
            maxNormalVol = normalData.maxVolume;
        }
        if (mirrorData != null && mirrorData.clips.Count > 0)
        {
            mirrorBgmSource.clip = mirrorData.clips[0];
            maxMirrorVol = mirrorData.maxVolume;
        }
        if (timeData != null && timeData.clips.Count > 0)
        {
            timeBgmSource.clip = timeData.clips[0];
            maxTimeVol = timeData.maxVolume;
        }

        normalBgmSource.volume = 0;
        mirrorBgmSource.volume = 0;
        timeBgmSource.volume = 0;

        normalBgmSource.Play();
        mirrorBgmSource.Play();
    }

    public void SwitchSyncedWorldBGM(WorldType targetWorld)
    {
        if (syncCrossfadeCo != null) StopCoroutine(syncCrossfadeCo);

        if (targetWorld == WorldType.Time && timeBgmSource.clip != null && normalBgmSource.clip != null)
        {
            float totalDuration = normalBgmSource.clip.length;
            float elapsed = normalBgmSource.time;

            float targetTime = totalDuration - elapsed;
            targetTime = Mathf.Clamp(targetTime, 0f, timeBgmSource.clip.length - 0.01f);

            timeBgmSource.time = targetTime;
            timeBgmSource.Play();
        }

        syncCrossfadeCo = StartCoroutine(CrossfadeSyncedBGM(targetWorld));
    }

    private IEnumerator CrossfadeSyncedBGM(WorldType targetWorld)
    {
        float duration = 1.0f;
        float time = 0;

        float startNormal = normalBgmSource.volume;
        float startMirror = mirrorBgmSource.volume;
        float startTimeVol = timeBgmSource.volume;

        float targetNormal = (targetWorld == WorldType.Normal) ? maxNormalVol : 0;
        float targetMirror = (targetWorld == WorldType.Mirror) ? maxMirrorVol : 0;
        float targetTime = (targetWorld == WorldType.Time) ? maxTimeVol : 0;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / duration;

            normalBgmSource.volume = Mathf.Lerp(startNormal, targetNormal, t);
            mirrorBgmSource.volume = Mathf.Lerp(startMirror, targetMirror, t);
            timeBgmSource.volume = Mathf.Lerp(startTimeVol, targetTime, t);

            yield return null;
        }

        normalBgmSource.volume = targetNormal;
        mirrorBgmSource.volume = targetMirror;
        timeBgmSource.volume = targetTime;

        if (targetWorld != WorldType.Time)
        {
            timeBgmSource.Stop();
        }
    }

    public void StartBGM(string musicGroup)
    {
        audioShouldPlay = true;

        if (musicGroup == currentAudioGroupName)
            return;

        NextBGM(musicGroup);
    }
    public void NextBGM(string musicGroup)
    {
        audioShouldPlay = true;
        currentAudioGroupName = musicGroup;

        if (currentAudioCo != null)
            StopCoroutine(currentAudioCo);

        currentAudioCo = StartCoroutine(SwitchMusicCo(musicGroup));
    }
    public void StopBGM()
    {
        audioShouldPlay = false;

        StartCoroutine(FadeVolumeCo(audioSource, 0, 1));

        if (currentAudioCo != null)
            StopCoroutine(currentAudioCo);
    }

    private IEnumerator SwitchMusicCo(string musicGroup)
    {
        AudioClipData data = audioDataBase.Get(musicGroup);
        AudioClip nextMusic = data.GetRandomClip();

        if (data == null && data.clips.Count == 0)
        {
            Debug.Log("No audio found for group - " + musicGroup);
            yield break;
        }

        if (data.clips.Count > 1)
        {
            while (nextMusic == lastMusicPlayed)
                nextMusic = data.GetRandomClip();
        }

        if (audioSource.isPlaying)
            yield return FadeVolumeCo(audioSource, 0, 1f);

        lastMusicPlayed = nextMusic;
        audioSource.clip = nextMusic;
        audioSource.volume = 0;
        audioSource.Play();

        StartCoroutine(FadeVolumeCo(audioSource, data.maxVolume, 1f));
    }

    private IEnumerator FadeVolumeCo(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float time = 0;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    public void PlaySFX(string soundName, AudioSource sfxSource, float DistanceToHearSound = 5)
    {
        if (player == null)
            player = Player.instance.transform;

        var data = audioDataBase.Get(soundName);
        if (data == null)
        {
            Debug.Log("Attempt to play sound - " + soundName);
            return;
        }

        var clip = data.GetRandomClip();
        if (clip == null)
            return;

        // ========================================================
        // 【核心修复 2】：无论怪物/玩家自身的组件有没有连 Mixer，
        // 只要通过这里播放，代码直接强制把它扣进 SFX 混音组中！
        // ========================================================
        if (sfxSource != null && sfxGroup != null && sfxSource.outputAudioMixerGroup != sfxGroup)
        {
            sfxSource.outputAudioMixerGroup = sfxGroup;
        }

        float maxVolume = data.maxVolume;
        float distance = Vector2.Distance(sfxSource.transform.position, player.position);
        float t = Mathf.Clamp01(1 - (distance / DistanceToHearSound));

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.volume = Mathf.Lerp(0, maxVolume, t * t);
        sfxSource.PlayOneShot(clip);
    }

    public void PlayGlobalSFX(string soundName)
    {
        var data = audioDataBase.Get(soundName);
        if (data == null) return;

        var clip = data.GetRandomClip();
        if (clip == null) return;

        Debug.Log("Playing Audio - " + soundName);

        // 安全防漏保险
        if (sfxSource != null && sfxGroup != null) sfxSource.outputAudioMixerGroup = sfxGroup;

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.volume = data.maxVolume;
        sfxSource.PlayOneShot(clip);
    }
}
