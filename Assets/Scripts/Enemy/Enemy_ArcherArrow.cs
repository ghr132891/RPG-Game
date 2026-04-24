using UnityEngine;

public class Enemy_ArcherArrow : MonoBehaviour, ICounterable
{
    [SerializeField] private LayerMask whatIsTarget;

    private Collider2D col;
    private Rigidbody2D rb;
    private Entity_Combat combat;

    public bool CanBeCountered => true;

    public void SetUpArrow(float xVelocity, Entity_Combat combat)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.linearVelocity = new Vector2(xVelocity, 0);
        this.combat = combat;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // check if the arrow hit the target
        if(((1 <<col.gameObject.layer) & whatIsTarget) != 0)
        {
            //damage the target
        }
    }

    public void HandleCounter()
    {
        //throw new System.NotImplementedException();
    }
}
