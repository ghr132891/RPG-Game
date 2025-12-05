using System;
using UnityEngine;


[Serializable]
public class DamageScaleData
{
    [Header("Damge")]
    public float phyiscal;
    public float elemental;

    [Header("Chill")]
    public float chillDuration;
    public float chillSlowMulitplier;

    [Header("Burn")]
    public float burnDuration;
    public float burnDamageScale;

    [Header("Shock")]
    public float shockDuration;
    public float shockDamageScale;
    public float shockCharge;




}

