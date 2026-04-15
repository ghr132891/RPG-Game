using UnityEngine;

public class TimeStopTarget : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;

    private Vector2 storedVelocity; // 记录被冻结前的速度
    private RigidbodyType2D originalBodyType; // 记录原本的刚体类型（解决 isKinematic 过时问题）

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // 监听世界切换事件
        if (WorldManager.Instance != null)
            WorldManager.Instance.OnWorldChanged += CheckTimeStop;
    }

    private void CheckTimeStop(WorldType worldType)
    {
        if (worldType == WorldType.Time)
        {
            // --- 1. 冻结动画 ---
            if (anim != null) anim.speed = 0;

            // --- 2. 冻结物理与速度 ---
            if (rb != null)
            {
                storedVelocity = rb.linearVelocity;
                originalBodyType = rb.bodyType; // 记住原本的类型（通常是 Dynamic）

                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic; // 替换过时的 isKinematic = true
            }

            // --- 3. 冻结AI逻辑 (如果有的话) ---
            var enemyScript = GetComponent<Enemy>();
            if (enemyScript != null) enemyScript.enabled = false;
        }
        else
        {
            // --- 恢复正常 ---
            if (anim != null) anim.speed = 1;

            if (rb != null)
            {
                rb.bodyType = originalBodyType; // 还原原本的类型（替换过时的 isKinematic = false）
                rb.linearVelocity = storedVelocity;   // 恢复之前的速度
            }

            var enemyScript = GetComponent<Enemy>();
            if (enemyScript != null) enemyScript.enabled = true;
        }
    }

    private void OnDestroy()
    {
        // 记得注销事件，防止报错
        if (WorldManager.Instance != null)
            WorldManager.Instance.OnWorldChanged -= CheckTimeStop;
    }
}
