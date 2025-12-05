using System.Collections;
using UnityEngine;

public class Entity_StatusHandler : MonoBehaviour
{
    private Entity entity;
    private Entity_VFX entityVFX;
    private Entity_Stats entityStats;
    private Entity_Health entityHealth;
    private ElementType currentEffect = ElementType.None;

    [Header("Shock Effect Details")]
    [SerializeField] private GameObject lightningStrikeVFX;
    [SerializeField] private float currentCharge;
    [SerializeField] private float maxmumCharge = 1;
    private Coroutine ShockCo;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        entityVFX = GetComponent<Entity_VFX>();
        entityStats = GetComponent<Entity_Stats>();
        entityHealth = GetComponent<Entity_Health>();

    }

    public void ApplyStatusEffect(ElementType element, ElementalEffectData effectData)
    {
        if (element == ElementType.Ice && CanBeApplied(ElementType.Ice))
        {
            ApplyChillEffect(effectData.chillDuration, effectData.chillSlowMulitplier);
        }

        if (element == ElementType.Fire && CanBeApplied(ElementType.Fire))
        {
            ApplyBurnedEffect(effectData.burnDuration, effectData.burnDamage);
        }

        if (element == ElementType.Lightning && CanBeApplied(ElementType.Lightning))
        {
            ApplyShockEffect(effectData.shockDuration, effectData.shockDamage, effectData.shockCharge);
        }

    }

    public void ApplyShockEffect(float duration, float damage, float charge)
    {
        float lightningResistance = entityStats.GetElementalResistance(ElementType.Lightning);
        float finalCharge = charge * (1 - lightningResistance);
        currentCharge += finalCharge;

        if (currentCharge >= maxmumCharge)
        {
            DoLightningStrike(damage);
            StopShockEfect();
            return;

        }
        if (ShockCo != null)
            StopCoroutine(ShockCo);

        ShockCo = StartCoroutine(ShockEffectCo(duration));


    }

    public void StopShockEfect()
    {
        currentEffect = ElementType.None;
        currentCharge = 0;
        entityVFX.StopAllVFX();
    }

    private void DoLightningStrike(float damage)
    {
        Instantiate(lightningStrikeVFX, transform.position, Quaternion.identity);
        entityHealth.ReduceHealth(damage);
    }

    public void ApplyBurnedEffect(float duration, float fireDamage)
    {
        float fireResistance = entityStats.GetElementalResistance(ElementType.Fire);
        float finalDamage = fireDamage * (1 - fireResistance);


        StartCoroutine(BurnedEffectCo(duration, finalDamage));
    }

    //电元素：电击
    private IEnumerator ShockEffectCo(float duration)
    {
        currentEffect = ElementType.Lightning;
        entityVFX.PlayOnStatusVFX(duration, ElementType.Lightning);
        yield return new WaitForSeconds(duration);
        StopShockEfect();

    }


    //火元素：灼烧
    private IEnumerator BurnedEffectCo(float duration, float totalDamage)
    {
        currentEffect = ElementType.Fire;
        entityVFX.PlayOnStatusVFX(duration, ElementType.Fire);


        int ticksPerSecond = 2;
        int tickCount = Mathf.RoundToInt(ticksPerSecond * duration);

        float damagePerTick = totalDamage / tickCount;
        float tickInterval = 1f / ticksPerSecond;

        for (int i = 0; i < tickCount; i++)
        {
            entityHealth.ReduceHealth(damagePerTick);
            yield return new WaitForSeconds(tickInterval);

        }

        currentEffect = ElementType.None;

    }

    public void ApplyChillEffect(float duration, float slowMultiplier)
    {
        float iceResistance = entityStats.GetElementalResistance(ElementType.Ice);
        float finalDuration = duration * (1 - iceResistance);

        StartCoroutine(ChilledEffectCo(finalDuration, slowMultiplier));
    }
    //冰元素 ：减速
    private IEnumerator ChilledEffectCo(float duration, float slowMultiplier)
    {
        entity.SlowDownEntity(duration, slowMultiplier);
        currentEffect = ElementType.Ice;
        entityVFX.PlayOnStatusVFX(duration, ElementType.Ice);

        yield return new WaitForSeconds(duration);

        currentEffect = ElementType.None;

    }

    public bool CanBeApplied(ElementType element)
    {
        if (element == ElementType.Lightning && currentEffect == ElementType.Lightning)
            return true;

        return currentEffect == ElementType.None;
    }

}
