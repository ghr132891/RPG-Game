[System.Serializable]
public class ElementalEffectData
{
    public float chillDuration;
    public float chillSlowMulitplier;

    public float burnDuration;
    public float burnDamage;

    public float shockDuration;
    public float shockDamage;
    public float shockCharge;


    public ElementalEffectData(Entity_Stats entity_Stats,DamageScaleData damageScale)

    {
        chillDuration =  damageScale.chillDuration;
        chillSlowMulitplier = damageScale.chillSlowMulitplier;

        burnDuration = damageScale.burnDuration;
        burnDamage = entity_Stats.offense.fireDamage.GetValue() * damageScale.burnDamageScale ;

        shockDuration = damageScale.shockDuration;
        shockDamage = entity_Stats.offense.lightningDamage.GetValue() * damageScale.shockDamageScale ;
        shockCharge = damageScale.shockCharge;

    }





}


