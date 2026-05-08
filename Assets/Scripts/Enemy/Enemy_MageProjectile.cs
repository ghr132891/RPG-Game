using UnityEngine;

public class Enemy_MageProjectile : MonoBehaviour
{
    private Entity_Combat combat;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator anim;

    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private LayerMask whatCanCollideWith;

    public void SetupProjectile(Transform target, Entity_Combat combat)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();

        anim.enabled = false;
        this.combat = combat;

        Vector2 velocity = CalculateBallisticVelocity(transform.position, target.position);
        rb.linearVelocity = velocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & whatCanCollideWith) != 0)
        {
            combat.PerformAttackOnTarget(collision.transform);

            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            anim.enabled = true;
            col.enabled = false;
            Destroy(gameObject, 2);
        }
    }


    private Vector2 CalculateBallisticVelocity(Vector2 start, Vector2 end)
    {

        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

        float displacementY = end.y - start.y;
        float displacementX = end.x - start.x;

        float peakHieght = Mathf.Max(arcHeight, end.y - start.y + .1f); 

        float timeToApex = Mathf.Sqrt(2 * peakHieght / gravity);

        float timeFromApex = Mathf.Sqrt(2 * (peakHieght - displacementY) / gravity);

        float totalTime = timeToApex + timeFromApex;

        float velocityY = Mathf.Sqrt(2 * gravity * peakHieght);

        float velocityX = displacementX / totalTime;

        return new Vector2(velocityX, velocityY);
    }
}
