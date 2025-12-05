
using UnityEngine;
using UnityEngine.UI;

public class Entity_Health : MonoBehaviour, IDamagable
{
    private Slider healthBar;
    private Entity_VFX entityVFX;
    private Entity entity;
    private Entity_Stats entityStats;

    [SerializeField] protected float currentHealth;
    [SerializeField] protected bool isDead;
    [Header("Health Regen")]
    [SerializeField] private float regenInterval = 1;
    [SerializeField] private bool canRegenerateHealth = true;

    [Header("On Damage Knockbacl")]
    [SerializeField] private Vector2 knockbackPower = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 heavyKnockbackPower = new Vector2(7, 7);
    [SerializeField] private float knockbackDuration = .2f;
    [SerializeField] private float heavyKnockbackDuration = .5f;
    [Header("On Heavy Damage")]
    [SerializeField] private float heavyDamageTreshold = .3f;
    protected virtual void Awake()
    {
        entityVFX = GetComponent<Entity_VFX>();
        entity = GetComponent<Entity>();
        entityStats = GetComponentInChildren<Entity_Stats>();
        healthBar = GetComponentInChildren<Slider>();

        currentHealth = entityStats.GetMaxHealth();
        UpdateHealthBar();

        InvokeRepeating(nameof(RengenerateHealth),0,regenInterval);
    }

    public virtual bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (isDead)
            return false;

        if (AttackEvaded())
        {
            Debug.Log($"{gameObject.name} evaded the attack!");
            return false;
        }

        Entity_Stats atttackStats = damageDealer.GetComponent<Entity_Stats>();
        float armorReduction = atttackStats != null ? atttackStats.GetArmorReduction() : 0;


        float armorMitigation = entityStats.GetArmorMitigation(armorReduction);
        float physicalDamageTaken = damage * (1 - armorMitigation);

        float resistance = entityStats.GetElementalResistance(element);
        float elementalDamageTaken = elementalDamage * (1 - resistance);

        TakeKnockback(damageDealer, physicalDamageTaken);

        ReduceHealth(physicalDamageTaken + elementalDamageTaken);

        return true;
    }

    

    private bool AttackEvaded() => Random.Range(0, 100) < entityStats.GetEvasion();

    public void RengenerateHealth()
    {
        if(canRegenerateHealth ==false)
            return;

        float regenAmount = entityStats.resources.healthRegen.GetValue();
        IncreaseHealth(regenAmount);



    }

    public void IncreaseHealth(float healAmount)
    {
        if(isDead)
            return;

        float maxHealth = entityStats.GetMaxHealth();
        float newHealth = currentHealth + healAmount;
        currentHealth = Mathf.Min(newHealth, maxHealth);

        UpdateHealthBar();
    }

    public void ReduceHealth(float damage)
    {
        entityVFX?.PlayerOnDamageVfx();
        currentHealth -= damage;
        UpdateHealthBar();

        if (currentHealth <= 0)
            Die();
    }

    public float GetHealthPercent() => currentHealth / entityStats.GetMaxHealth();

    public void SetHealthToPercent(float percent)
    {
        currentHealth = entityStats.GetMaxHealth() * Mathf.Clamp01(percent);
        UpdateHealthBar();

    }

    private void Die()
    {
        isDead = true;
        entity.EntityDeath();
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.value = currentHealth / entityStats.GetMaxHealth();
    }


    private void TakeKnockback(Transform damageDealer, float finalDamage)
    {
        Vector2 knockback = CalculateKnockback(finalDamage, damageDealer);
        float duration = CalculateDuration(finalDamage);

        entity?.ReceiveKnockback(knockback, duration);
    }

    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;
        Vector2 knockback = IsHeavyDamage(damage) ? heavyKnockbackPower : knockbackPower;
        knockback.x = knockback.x * direction;


        return knockback;
    }
    private float CalculateDuration(float damage) => IsHeavyDamage(damage) ? heavyKnockbackDuration : knockbackDuration;
    private bool IsHeavyDamage(float damage) => damage / entityStats.GetMaxHealth() > heavyDamageTreshold;

}
