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

    [Header("Float Movement")]
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float floatRange = .1f;
    private Vector3 startPosition;
    [SerializeField] private GameObject interactToolTip;

    // 【新增】：用于标记玩家是否站在传送点范围内
    private bool isPlayerInZone = false;

    public RespawnType GetWayPointType() => wayPointType;

    //public bool SetCanBeTriggered(bool canBeTriggered) => this.canBeTriggered = canBeTriggered;

    public Vector3 GetPositionAndSetTriggerFalse()
    {
        canBeTriggered = false;
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
    private void Awake()
    {
        startPosition = interactToolTip.transform.position;
        interactToolTip.SetActive(false);
    }
    // 【新增】：持续检测，如果玩家在范围内，且按下 F 键，才执行你原有的传送逻辑
    private void Update()
    {
        HandleToolTipFloat();


        if (isPlayerInZone && canBeTriggered)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                GameManager.instance.ChangeScene(transferToScene, connectedWayPoint);
            }
        }
    }

    private void HandleToolTipFloat()
    {
        if (interactToolTip.activeSelf)
        {
            float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
            interactToolTip.transform.position = startPosition + new Vector3(0, yOffset);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canBeTriggered == false)
            return;
        interactToolTip.SetActive(true);

        // 【修改】：不再直接传送，而是标记玩家进入了区域
        // (顺手加了个判断，防止怪物/弓箭等碰到传送点也能触发 F 键判定)
        if (collision.GetComponent<Player>() != null)
        {
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        interactToolTip.SetActive(false);
        // 【修改】：玩家离开时，取消标记
        if (collision.GetComponent<Player>() != null)
        {
            isPlayerInZone = false;
        }

        // 完全保留你原有的离开重置逻辑
        canBeTriggered = true;
    }
}