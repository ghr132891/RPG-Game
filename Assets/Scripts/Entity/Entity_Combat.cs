using UnityEngine;

public class Entity_Combat : MonoBehaviour
{
    private Entity_VFX vfx;
    private Entity_Stats stats;

    public DamageScaleData basicAttackScale;


    [Header("Target Details")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius = 1;
    [SerializeField] private LayerMask whatIsTarget;

    
    private void Awake()
    {
        vfx = GetComponent<Entity_VFX>();
        stats = GetComponent<Entity_Stats>();
    }


    public void PerformAttack()
    {
        foreach (var target in GetDetectedColliders())
        {
            IDamagable damageble = target.GetComponent<IDamagable>();

            if (damageble == null)
                continue;

            AttackData attackData = stats.GetAttackData(basicAttackScale);
            Entity_StatusHandler entity_StatusHandler = target.GetComponent<Entity_StatusHandler>();


            float phyiscalDamage = attackData.phyiscalDamage;
            float elementalDamage = attackData.elementDamage;
            ElementType element = attackData.element;

            bool targetGotHit = damageble.TakeDamage(phyiscalDamage, elementalDamage, element, transform);

            if (element != ElementType.None)
                entity_StatusHandler?.ApplyStatusEffect(element, attackData.effectData);

            if (targetGotHit)
            {

                vfx.CreatOnHitVFX(target.transform, attackData.isCrit, element);
            }
        }
    }

    //public void ApplyStatusEffect(Transform target, ElementType element,float scaleFactor = 1)
    //{
    //    Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();

    //    if (statusHandler == null)
    //        return;

    //    if (element == ElementType.Ice && statusHandler.CanBeApplied(ElementType.Ice))
    //        statusHandler.ApplyChillEffect(defaultDuration, chillSlowMultiplier);

    //    if (element == ElementType.Fire && statusHandler.CanBeApplied(ElementType.Fire))
    //    {
    //        scaleFactor = fireScale;
    //        float fireDamage = stats.offense.fireDamage.GetValue() * scaleFactor;
    //        statusHandler.ApplyBurnedEffect(defaultDuration, fireDamage);
    //    }

    //    if(element == ElementType.Lightning && statusHandler.CanBeApplied(ElementType.Lightning))
    //    {
    //        scaleFactor = lightningScale;
    //        float lightningDamage = stats.offense.lightningDamage.GetValue() * scaleFactor;
    //        statusHandler.ApplyElectrifyEffect(defaultDuration, lightningDamage,electrifyEfectBuildUp );

    //    }
    //}

    protected Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, whatIsTarget);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }
}
