using JetBrains.Annotations;
using System.Collections;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;    
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeScene(string sceneName,RespawnType respawnType)
    {
        StartCoroutine(ChangeScnenCo(sceneName, respawnType));
    }
    private IEnumerator ChangeScnenCo(string sceneName,RespawnType respawnType)
    {
        // fade Effect

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(sceneName);

        yield return new WaitForSeconds(.2f);

        Vector3 position = GetWayPointPosition(respawnType);

        if (position != Vector3.zero)
            Player.instance.TeleportPlayer(position);
    }

    private Vector3 GetWayPointPosition(RespawnType type)
    {
        var wayPoints = FindObjectsByType<Object_WayPoint>(FindObjectsSortMode.None);

        foreach (var point in wayPoints)
        {
            if (point.GetWayPointType() == type)
            {
                point.SetCanBeTriggered(false);
                return point.GetPosition();
            }
             
        }
        return Vector3.zero;
    }
}
