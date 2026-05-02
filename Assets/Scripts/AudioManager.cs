using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDataBaseSo audioDataBase;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource sfxSource;

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
