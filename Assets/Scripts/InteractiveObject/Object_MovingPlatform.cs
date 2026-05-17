using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Object_MovingPlatform : MonoBehaviour
{
    [Header("移动路径设置")]
    public Transform[] waypoints;
    public float moveSpeed = 3f;
    public float idleTime = 1f;

    private int currentWaypointIndex = 0;
    private float idleTimer;
    private Rigidbody2D rb;

    private HashSet<Rigidbody2D> passengers = new HashSet<Rigidbody2D>();

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
    }

    private void FixedUpdate()
    {
        if (waypoints.Length == 0) return;

        if (idleTimer > 0)
        {
            idleTimer -= Time.fixedDeltaTime;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetWaypoint.position, moveSpeed * Time.fixedDeltaTime);
        Vector2 movement = newPosition - rb.position;

        rb.MovePosition(newPosition);

        // ==========================================
        // 【终极物理防滑修复区】
        // ==========================================
        foreach (Rigidbody2D passenger in passengers)
        {
            if (passenger != null)
            {
                // 1. 水平方向 (X轴)：必须时刻手动牵引，否则没有摩擦力会滑落
                float moveX = movement.x;

                // 2. 垂直方向 (Y轴) 核心逻辑：
                // 如果平台向上移动 (movement.y > 0)，物理引擎的刚体碰撞会自动把玩家稳稳顶上去。此时我们设为 0，防止把玩家“双重顶飞”。
                // 如果平台向下移动 (movement.y < 0)，为了防止失重脱离，我们手动把玩家“拉”下来。
                float moveY = movement.y < 0 ? movement.y : 0f;

                // 3. 应用位移
                passenger.position += new Vector2(moveX, moveY);
            }
        }

        if (Vector2.Distance(rb.position, targetWaypoint.position) < 0.05f)
        {
            idleTimer = idleTime;
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 确保是从【上方】踩上来的 (法线向下)
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y < -0.5f)
        {
            Rigidbody2D pRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (pRb != null && collision.gameObject.GetComponent<Entity>() != null)
            {
                passengers.Add(pRb);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D pRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (pRb != null)
        {
            passengers.Remove(pRb);
        }
    }
}