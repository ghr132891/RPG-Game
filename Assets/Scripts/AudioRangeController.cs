using UnityEngine;

public class AudioRangeController : MonoBehaviour
{
    private AudioSource source;
    private Transform player;

    [SerializeField] private float DistanceToHearSound = 15;
    [SerializeField] private bool showGizom;
    private float maxVolume;

    private void Start()
    {
        player = Player.instance.transform;
        source = GetComponent<AudioSource>();

        maxVolume = source.volume;
    }

    private void Update()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        float t = Mathf.Clamp01(1 - (distance / DistanceToHearSound));

        float targetVolume = Mathf.Lerp(0, maxVolume, t * t);
        source.volume = Mathf.Lerp(source.volume, targetVolume, Time.deltaTime * 2);
    }

    private void OnDrawGizmos()
    {
        if (showGizom)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, DistanceToHearSound);
        }
    }
}
