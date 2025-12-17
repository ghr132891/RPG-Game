using UnityEngine;

public class SkillObject_Base : MonoBehaviour
{

    [SerializeField] protected GameObject onHitVfx;
    [Space]
    [SerializeField] protected LayerMask whatIsEnemy;
    [SerializeField] protected Transform targetCheck;
    [SerializeField] protected float checkRadius = 1;
    protected Rigidbody2D rb;

    protected Animator anim;
    protected Entity_Stats playerStats;
    protected DamageScaleData damageScaleData;
    protected ElementType usedElement;
    protected bool targetGotHit;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected Collider2D[] GetEnemiesAround(Transform t, float radius)
    {
        return Physics2D.OverlapCircleAll(t.position, radius, whatIsEnemy);

    }

    protected void DamageEnemiesInRadius(Transform t, float radius)
    {
        foreach (var target in GetEnemiesAround(t, radius))
        {
            IDamagable damagable = target.GetComponent<IDamagable>();

            if (damagable == null)
                continue;

            AttackData attackData = playerStats.GetAttackData(damageScaleData);
            Entity_StatusHandler entity_StatusHandler = target.GetComponent<Entity_StatusHandler>();

            float physDamage = attackData.phyiscalDamage;
            float elemDamage = attackData.elementDamage;
            ElementType element = attackData.element;


            targetGotHit = damagable.TakeDamage(physDamage, elemDamage, element, transform);

            if (element != ElementType.None)
                entity_StatusHandler?.ApplyStatusEffect(element,attackData.effectData);

            if (targetGotHit)
                Instantiate(onHitVfx,target.transform.position,Quaternion.identity);

            usedElement = element;

            
        }
       
        
        

    }

    protected Transform FindClosestTarget()
    {
        Transform target = null; 
        float closestDistance = Mathf.Infinity;

        foreach(var enemy in GetEnemiesAround(transform, 10))
        {
            float distance = Vector2.Distance(transform.position,enemy.transform.position);

            if(distance< closestDistance)
            {
                target = enemy.transform;
                closestDistance = distance;

            }

        }

        return target;


    }

    protected virtual void OnDrawGizmos()
    {
        if (targetCheck == null)
            targetCheck = transform;


        Gizmos.DrawWireSphere(targetCheck.position, checkRadius);

    }

}
