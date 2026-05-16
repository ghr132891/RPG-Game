using UnityEngine;

public class Object_CheckPoint : MonoBehaviour, ISaveable
{

    [SerializeField] private string checkPointID;
    [SerializeField] private Transform respawnPoint;

    [Header("CheckPoint Light")]
    [SerializeField] private GameObject fireLightObject;

    private Animator anim;
    private AudioSource fireAudioSource;

    public bool isActive { get; private set; }

    private void Awake()
    {

        anim = GetComponentInChildren<Animator>();


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

        // ==========================================
        // 【新增】：根据激活状态，同步控制灯光的亮灭
        // ==========================================
        if (fireLightObject != null)
        {
            fireLightObject.SetActive(activate);
        }

        if (fireAudioSource != null)
        {

            if (isActive && fireAudioSource.isPlaying == false)
                fireAudioSource.Play();

            if (isActive == false)
                fireAudioSource.Stop();
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            ActivateCheckPoint(true);
        }
    }

    public void LoadData(GameData gameData)
    {
        //读取逻辑
        bool isUnlocked = false;
        if (gameData.unlockedCheckPoints.TryGetValue(checkPointID, out isUnlocked))
        {
            ActivateCheckPoint(isUnlocked); // 如果存档里有记录，就按照记录的来
        }
        else
        {
            ActivateCheckPoint(false); // 如果存档里没记录，默认关闭
        }
    }

    public void SaveData(ref GameData gameData)
    {
        if (isActive == false)
            return;

        if (gameData.unlockedCheckPoints.ContainsKey(checkPointID) == false)
            gameData.unlockedCheckPoints.Add(checkPointID, true);
    }
}
