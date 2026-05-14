using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDataBaseSo audioDataBase;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource sfxSource;

    // ==========================================
    // 【新增】世界同步 BGM 专属音源与变量
    // ==========================================
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

        // 【新增】：自动创建 3 个平行的音频源用于垂直混音
        normalBgmSource = gameObject.AddComponent<AudioSource>();
        normalBgmSource.loop = true;

        mirrorBgmSource = gameObject.AddComponent<AudioSource>();
        mirrorBgmSource.loop = true;

        timeBgmSource = gameObject.AddComponent<AudioSource>();
        timeBgmSource.loop = true;
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

    // ==========================================
    // 【新增】初始化同步音乐（由 WorldAudioManager 调用）
    // ==========================================
    public void InitSyncedWorldBGM(string normalBGM, string mirrorBGM, string timeBGM)
    {

        // 【核心修复】：检查当前播放的音乐是否与即将播放的音乐完全相同
        if (normalBGM == currentNormalName && mirrorBGM == currentMirrorName && timeBGM == currentTimeName)
        {
            Debug.Log("检测到相同音乐，保持无缝持续播放。");
            return; // 直接退出，不执行 Stop()，音乐就不会断
        }

        // 更新当前的音乐记录
        currentNormalName = normalBGM;
        currentMirrorName = mirrorBGM;
        currentTimeName = timeBGM;

        // 【核心修复 1】：彻底重置旧系统的状态，防止 Update 循环自动重启旧音乐
        audioShouldPlay = false;
        currentAudioGroupName = null;

        // 停止主音源（处理 MainMenu 或其他单曲的音源）
        if (audioSource.isPlaying)
            audioSource.Stop();

        // 停止可能正在运行的旧淡入淡出协程
        if (currentAudioCo != null)
            StopCoroutine(currentAudioCo);

        // 【核心修复 2】：加载并初始化三世界同步音轨
        var normalData = audioDataBase.Get(normalBGM);
        var mirrorData = audioDataBase.Get(mirrorBGM);
        var timeData = audioDataBase.Get(timeBGM);

        // 载入音频数据
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

        // 关键：普通和镜像世界在第 0 帧同时播放，进度绝对同步！
        normalBgmSource.volume = 0;
        mirrorBgmSource.volume = 0;
        timeBgmSource.volume = 0;

        normalBgmSource.Play();
        mirrorBgmSource.Play();
        // （时间世界先不播放，等进入时再精准定位时间）
    }

    // ==========================================
    // 【新增】处理世界切换与时间轴反转逻辑
    // ==========================================
    public void SwitchSyncedWorldBGM(WorldType targetWorld)
    {
        if (syncCrossfadeCo != null) StopCoroutine(syncCrossfadeCo);

        // 核心机制：当切换到时间世界时，计算时间逆转
        if (targetWorld == WorldType.Time && timeBgmSource.clip != null && normalBgmSource.clip != null)
        {
            float totalDuration = normalBgmSource.clip.length; // 基础世界音频总时长
            float elapsed = normalBgmSource.time;              // 该世界已经播放的时长

            // 你的核心公式：总时长 - 已播放时长
            float targetTime = totalDuration - elapsed;

            // 安全限制：防止超过时间音频本身的长度导致报错
            targetTime = Mathf.Clamp(targetTime, 0f, timeBgmSource.clip.length - 0.01f);

            // 从这个倒转的时间点开始播放！
            timeBgmSource.time = targetTime;
            timeBgmSource.Play();
        }

        // 开始音量交叉淡入淡出
        syncCrossfadeCo = StartCoroutine(CrossfadeSyncedBGM(targetWorld));
    }

    private IEnumerator CrossfadeSyncedBGM(WorldType targetWorld)
    {
        float duration = 1.0f; // 渐变时长
        float time = 0;

        float startNormal = normalBgmSource.volume;
        float startMirror = mirrorBgmSource.volume;
        float startTimeVol = timeBgmSource.volume;

        // 确定最终目标音量：只有当前世界的音乐才会有声音
        float targetNormal = (targetWorld == WorldType.Normal) ? maxNormalVol : 0;
        float targetMirror = (targetWorld == WorldType.Mirror) ? maxMirrorVol : 0;
        float targetTime = (targetWorld == WorldType.Time) ? maxTimeVol : 0;

        // 使用 unscaledDeltaTime 保证不受你时间魔法的 TimeScale 影响
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

        // 当不在时间世界时，彻底停止时间音乐以节省性能。
        // 而普通和镜像音乐保持静音播放，进度继续同步推进！
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

        if(data == null && data.clips.Count == 0)
        {
            Debug.Log("No audio found for group - " + musicGroup );
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

        float maxVolume = data.maxVolume;
        float distance = Vector2.Distance(sfxSource.transform.position, player.position);
        float t = Mathf.Clamp01(1 - (distance / DistanceToHearSound));


        //sfxSource.clip = clip;
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

        sfxSource.pitch = Random.Range(0.95f, 1.05f);
        sfxSource.volume = data.maxVolume;
        sfxSource.PlayOneShot(clip);


    }

}
