using UnityEngine;

public class Object_CheckPoint : MonoBehaviour,ISaveable
{

    private Object_CheckPoint[] allCheckPoints;
    private Animator anim;

    private void Awake()
    {

        anim = GetComponentInChildren<Animator>();
        allCheckPoints = FindObjectsByType<Object_CheckPoint>(FindObjectsSortMode.None);
    }

    public void ActivateCheckPoint(bool activate)
    {
        anim.SetBool("isActive", activate);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        foreach (var point in allCheckPoints)
            point.ActivateCheckPoint(false);

        SaveManager.instance.GetGameData().savedCheckPoint = (Vector2)transform.position;
        ActivateCheckPoint(true);
    }

    public void LoadData(GameData gameData) 
    {
        bool active = gameData.savedCheckPoint == transform.position;
        ActivateCheckPoint(active);

        if(active)
            Player.instance.TeleportPlayer(transform.position);

    }

    public void SaveData(ref GameData gameData)
    {
        
    }
}
