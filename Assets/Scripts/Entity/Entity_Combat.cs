using System;
using UnityEngine;

public class Entity_Combat : MonoBehaviour
{
    public event Action<float> OnDoingPhysiclDamage;

    private Entity_VFX vfx;
    private Entity_SFX sfx;
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
        sfx = GetComponent<Entity_SFX>();
    }


    public void PerformAttack()
    {
        bool targetGotHit = false;

        foreach (var target in GetDetectedColliders(whatIsTarget))
        {
            IDamagable damageble = target.GetComponent<IDamagable>();

            if (damageble == null)
                continue;

            AttackData attackData = stats.GetAttackData(basicAttackScale);
            Entity_StatusHandler entity_StatusHandler = target.GetComponent<Entity_StatusHandler>();


            float phyiscalDamage = attackData.phyiscalDamage;
            float elementalDamage = attackData.elementDamage;
            ElementType element = attackData.element;

             targetGotHit = damageble.TakeDamage(phyiscalDamage, elementalDamage, element, transform);

            if (element != ElementType.None)
                entity_StatusHandler?.ApplyStatusEffect(element, attackData.effectData);

            if (targetGotHit)
            {
                OnDoingPhysiclDamage?.Invoke(phyiscalDamage);
                vfx.CreatOnHitVFX(target.transform, attackData.isCrit, element);
                sfx?.PlayAttackHit();
            }
        }

        if(targetGotHit == false)
            sfx?.PlayAttackMiss(); 

    }

    public void PerformAttackOnTarget(Transform target)
    {
        
            IDamagable damageble = target.GetComponent<IDamagable>();

            if (damageble == null)
                return;

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
                OnDoingPhysiclDamage?.Invoke(phyiscalDamage);
                vfx.CreatOnHitVFX(target.transform, attackData.isCrit, element);
            }

        
    }

    protected Collider2D[] GetDetectedColliders(LayerMask whatToDetect)
    {
        return Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, whatToDetect);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }
}
