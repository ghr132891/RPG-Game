using UnityEngine;
using UnityEngine.SceneManagement;


public class Object_WayPoint : MonoBehaviour
{
    [SerializeField] private string transferToScene;
    [Space]
    [SerializeField] private RespawnType wayPointType;
    [SerializeField] private RespawnType connectedWayPoint;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private bool canBeTriggered = true;

    public RespawnType GetWayPointType() => wayPointType;

    public bool SetCanBeTriggered(bool canBeTriggered) => this.canBeTriggered = canBeTriggered;

    public Vector3 GetPosition()
    {
        return respawnPoint == null ? transform.position : respawnPoint.position;
    }
    private void OnValidate()
    {
        gameObject.name = "Object_WayPoint - " + wayPointType.ToString() + " - " + transferToScene;

        if (wayPointType == RespawnType.Enter)
            connectedWayPoint = RespawnType.Exit;

        if (wayPointType == RespawnType.Exit)
            connectedWayPoint = RespawnType.Enter;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canBeTriggered == false)
            return;

        SaveManager.instance.SaveGame();
        GameManager.instance.ChangeScene(transferToScene, connectedWayPoint);


    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        canBeTriggered = true;
    }
}
