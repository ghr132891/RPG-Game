using UnityEngine;

public class Object_CheckPoint : MonoBehaviour, ISaveable
{

    [SerializeField] private string checkPointID;
    [SerializeField] private Transform respawnPoint;

    private Animator anim;
    private AudioSource fireAudioSource;
    public bool isActive { get; private set; }

    private void Awake()
    {

        anim = GetComponentInChildren<Animator>();
        fireAudioSource = GetComponent<AudioSource>();

    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(checkPointID))
        {
            checkPointID = System.Guid.NewGuid().ToString();
        }
#endif
    }

    public string GetCheckPointID() => checkPointID;
    public Vector3 GetPosition() => respawnPoint == null ? transform.position : respawnPoint.position;

    public void ActivateCheckPoint(bool activate)
    {
        isActive = activate;
        anim.SetBool("isActive", activate);

        if (isActive && fireAudioSource.isPlaying == false)
            fireAudioSource.Play();

        if (isActive == false)
            fireAudioSource.Stop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        ActivateCheckPoint(true);
    }

    public void LoadData(GameData gameData)
    {
        bool active = gameData.unlockedCheckPoints.TryGetValue(checkPointID, out active);
        ActivateCheckPoint(active);
    }

    public void SaveData(ref GameData gameData)
    {
        if (isActive == false)
            return;

        if (gameData.unlockedCheckPoints.ContainsKey(checkPointID) == false)
            gameData.unlockedCheckPoints.Add(checkPointID, true);
    }
}
