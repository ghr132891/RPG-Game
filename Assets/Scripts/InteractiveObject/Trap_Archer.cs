using UnityEngine;

public class Trap_Archer : MonoBehaviour
{
    [Header("预制体与位置")]
    public GameObject arrowPrefab;
    public Transform shotPoint;

    [Header("射击属性")]
    public float shootInterval = 2f;
    [Tooltip("飞行的绝对速度 (正数即可)")]
    public float arrowSpeed = 15f;
    [Tooltip("机关箭的伤害值")]
    public int arrowDamage = 1;

    [Tooltip("初始状态：勾选则向右射，不勾选则向左射")]
    public bool shootRight = false;

    private float shootTimer;

    // 【新增】：记录这个机关所属的镜像房间
    private MirrorZone myMirrorZone;

    private void Start()
    {
        // 自动向上寻找挂载了 MirrorZone 的父节点
        // 只要你在 Unity 里把机关放在 MirrorZone 的层级之下，它就会自动绑定成功
        myMirrorZone = GetComponentInParent<MirrorZone>();
    }

    private void Update()
    {
        shootTimer += Time.deltaTime;

        if (shootTimer >= shootInterval)
        {
            shootTimer = 0f;
            Shoot();
        }
    }

    private void Shoot()
    {
        if (arrowPrefab == null || shotPoint == null) return;

        // ==========================================
        // 【核心修改】：只跟自己所在的房间同生共死
        // 1. 如果这个机关没放在镜像房间里 (myMirrorZone 为空)，就正常射击
        // 2. 如果放在了镜像房间里，且房间当前是翻转的，就反向射击
        // ==========================================
        bool actualShootRight = shootRight;
        if (myMirrorZone != null && myMirrorZone.isCurrentlyMirrored)
        {
            actualShootRight = !shootRight;
        }

        // 生成箭矢 (依然保留了你想要的 shotPoint.rotation，支持任意角度旋转)
        GameObject newArrow = Instantiate(arrowPrefab, shotPoint.position, shotPoint.rotation);

        Enemy_ArcherArrow arrowScript = newArrow.GetComponent<Enemy_ArcherArrow>();
        if (arrowScript != null)
        {
            float finalVelocity = actualShootRight ? arrowSpeed : -arrowSpeed;
            arrowScript.SetUpTrapArrow(finalVelocity, arrowDamage);
        }
    }
}