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

    [Tooltip("勾选则向右射，不勾选则向左射")]
    public bool shootRight = false;

    private float shootTimer;

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

        // 生成箭矢
        GameObject newArrow = Instantiate(arrowPrefab, shotPoint.position, Quaternion.identity);

        Enemy_ArcherArrow arrowScript = newArrow.GetComponent<Enemy_ArcherArrow>();
        if (arrowScript != null)
        {
            // 根据方向决定速度是正还是负
            float finalVelocity = shootRight ? arrowSpeed : -arrowSpeed;

            // 调用我们刚写的机关专用初始化！
            arrowScript.SetUpTrapArrow(finalVelocity, arrowDamage);
        }
    }
}