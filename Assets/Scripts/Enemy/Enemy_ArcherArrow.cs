using UnityEngine;

public class Enemy_ArcherArrow : MonoBehaviour, ICounterable
{
    [SerializeField] private LayerMask whatIsTarget;

    private Collider2D col;
    private Rigidbody2D rb;
    private Entity_Combat combat;
    private Animator anim;

    //用于标记这是否是一支“机关射出”的箭
    private bool isTrapArrow;
    private int trapDamage;

    public bool CanBeCountered => true;

    //这是原来敌人调用的初始化方法（保持不变，不影响敌人！）
    public void SetUpArrow(float xVelocity, Entity_Combat combat)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();

        rb.linearVelocity = new Vector2(xVelocity, 0);
        this.combat = combat;
        this.isTrapArrow = false;

        if (rb.linearVelocity.x < 0)
            transform.Rotate(0, 180, 0);
    }

    //这是专门给“墙上机关”调用的初始化方法！不需要 Combat！
    public void SetUpTrapArrow(float xVelocity, int damage)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();

        rb.linearVelocity = new Vector2(xVelocity, 0);
        this.trapDamage = damage;
        this.isTrapArrow = true;

        if (rb.linearVelocity.x < 0)
            transform.Rotate(0, 180, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // check if the arrow hit the target
        if (((1 << collision.gameObject.layer) & whatIsTarget) != 0)
        {
            if (isTrapArrow)
            {
                Player player = collision.GetComponent<Player>();
                Enemy enemy = collision.GetComponent<Enemy>();
                if (player != null)
                {
                    player.health.TakeDamage(trapDamage,0,0,transform);
                }
                else if(enemy != null)
                {
                    enemy.health.TakeDamage(0, 0, 0, transform);
                }

                    Debug.Log($"机关箭矢命中了目标，造成了 {trapDamage} 点伤害！");
            }
            else if (combat != null)
            {
                // 原本敌人的伤害逻辑
                combat.PerformAttackOnTarget(collision.transform);
            }

            StuckIntoTarget(collision.transform);
        }
    }

    private void StuckIntoTarget(Transform target)
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        col.enabled = false;

        // 加个安全判断，防止某些箭预制体上没挂 Animator 导致这里又报错
        if (anim != null) anim.enabled = false;

        transform.parent = target;

        Destroy(gameObject, 2);
    }

    public void HandleCounter()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * -1, 0);
        transform.Rotate(0, 180, 0);

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player");
        whatIsTarget |= (1 << enemyLayer);
        whatIsTarget &= ~(1 << playerLayer);
    }
}
