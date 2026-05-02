using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDataBaseSo audioDataBase;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource sfxSource;

    private AudioClip lastMusicPlayed;
    private string currentAudioGroupName;
    private Coroutine currentAudioCo;
    [SerializeField] private bool audioShouldPlay;

    private Transform player;

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
            time += Time.deltaTime;
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
