using UnityEngine;

public class Enemy_ArcherArrow : MonoBehaviour, ICounterable
{
    [SerializeField] private LayerMask whatIsTarget;

    private Collider2D col;
    private Rigidbody2D rb;
    private Entity_Combat combat;
    private Animator anim;

    public bool CanBeCountered => true;

    public void SetUpArrow(float xVelocity, Entity_Combat combat)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();

        rb.linearVelocity = new Vector2(xVelocity, 0);
        this.combat = combat;

        if (rb.linearVelocity.x < 0)
            transform.Rotate(0,180,0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // check if the arrow hit the target
        if(((1 <<collision.gameObject.layer) & whatIsTarget) != 0)
        {
            //damage the target
            combat.PerformAttackOnTarget(collision.transform);
            StuckIntoTarget(collision.transform);
        }
    }

    private void StuckIntoTarget(Transform target)
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;
        anim.enabled = false;

        transform.parent = target;

        Destroy(gameObject, 2);
    }
    public void HandleCounter()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * -1, 0);
        transform.Rotate(0,180,0);

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");
        whatIsTarget |= (1 <<enemyLayer);// whatIsTarget = whatIsTarget | (1 << enemyLayer);
        whatIsTarget &= ~(1 << playerLayer);
    }
}
