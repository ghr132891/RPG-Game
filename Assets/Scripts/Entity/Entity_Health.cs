
using System;
using UnityEngine;
using UnityEngine.UI;

public class Entity_Health : MonoBehaviour, IDamagable
{
    public event Action OnTakingDamage;
    public event Action OnHealthUpdate;
    public event Action OnDie;

    private Slider healthBar;
    private Entity_VFX entityVFX;
    private Entity entity;
    private Entity_Stats entityStats;
    private Entity_DropManager dropManager;

    private bool minHealthBarActive;
    [SerializeField] protected float currentHealth;
    [Header("Health Regen")]
    [SerializeField] private float regenInterval = 1;
    [SerializeField] private bool canRegenerateHealth = true;
    public float lastDamageTaken { get; private set; }
    public bool isDead { get; private set; }
    protected bool canTakeDamage = true;

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
        dropManager = GetComponent<Entity_DropManager>();

    }

    protected virtual void Start()
    {
        SetUpHealth();
        
    }

    private void SetUpHealth()
    {
        if (entityStats == null)
            return;

        currentHealth = entityStats.GetMaxHealth();
        OnHealthUpdate += UpdateHealthBar;
        UpdateHealthBar();
        InvokeRepeating(nameof(RengenerateHealth), 0, regenInterval);
    }

    public virtual bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (isDead || canTakeDamage == false)
            return false;

        if (AttackEvaded())
        {
            Debug.Log($"{gameObject.name} evaded the attack!");
            return false;
        }

        Entity_Stats atttackStats = damageDealer.GetComponent<Entity_Stats>();
        float armorReduction = atttackStats != null ? atttackStats.GetArmorReduction() : 0;
        float armorMitigation = entityStats != null ? entityStats.GetArmorMitigation(armorReduction) : 0;
        float resistance = entityStats != null ? entityStats.GetElementalResistance(element) : 0;


        float physicalDamageTaken = damage * (1 - armorMitigation);

        float elementalDamageTaken = elementalDamage * (1 - resistance);

        TakeKnockback(damageDealer, physicalDamageTaken);

        ReduceHealth(physicalDamageTaken + elementalDamageTaken);

        lastDamageTaken = physicalDamageTaken + elementalDamageTaken;

        OnTakingDamage?.Invoke();
        return true;
    }

    public bool SetCanTakeDamage(bool canTakeDamage) => this.canTakeDamage = canTakeDamage;

    private bool AttackEvaded()
    {
        if (entityStats == null)
            return false;
        else
            return UnityEngine.Random.Range(0, 100) < entityStats.GetEvasion();
    }

    public void RengenerateHealth()
    {
        if (canRegenerateHealth == false)
            return;

        float regenAmount = entityStats.resources.healthRegen.GetValue();
        IncreaseHealth(regenAmount);



    }

    public void IncreaseHealth(float healAmount)
    {
        if (isDead)
            return;

        float maxHealth = entityStats.GetMaxHealth();
        float newHealth = currentHealth + healAmount;

        currentHealth = Mathf.Min(newHealth, maxHealth);

        OnHealthUpdate?.Invoke();
    }

    public void ReduceHealth(float damage)
    {
        currentHealth -= damage;

        entityVFX?.PlayerOnDamageVfx();
        OnHealthUpdate?.Invoke();

        if (currentHealth <= 0)
            Die();
    }

    public float GetHealthPercent() => currentHealth / entityStats.GetMaxHealth();

    public void SetHealthToPercent(float percent)
    {
        currentHealth = entityStats.GetMaxHealth() * Mathf.Clamp01(percent);
        OnHealthUpdate?.Invoke();

    }

    protected virtual void Die()
    {
        isDead = true;
        entity.EntityDeath();
        //dropManager?.DropItems();
        OnDie?.Invoke();
    }

    public void Revive()
    {
        isDead = false;
        canTakeDamage = true;
        SetHealthToPercent(1f); // 强行恢复 100% 的血量，并且这句代码自带了 UI 刷新通知！
    }

    public float GetCurrentHealth() => currentHealth; 


    private void UpdateHealthBar()
    {
        if (healthBar == null && healthBar.transform.parent.gameObject.activeSelf == false)
            return;

        healthBar.value = currentHealth / entityStats.GetMaxHealth();
    }

    public void EnableHealthBar(bool enable) => healthBar?.transform.parent.gameObject.SetActive(enable); 

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
    private bool IsHeavyDamage(float damage)
    {
        if (entityStats == null)
            return false;
        else
            return damage / entityStats.GetMaxHealth() > heavyDamageTreshold;
    }



}
