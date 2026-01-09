using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/Buff Effect", fileName = "Item Effect data - Buff")]
public class ItemEffect_Buff : ItemEffect_DataSo
{
    [SerializeField] private BuffEffectData[] buffToApply;
    [SerializeField] private float duration;
    [SerializeField] private string source = Guid.NewGuid().ToString();

    private Player_Stats playerStats;

    public override bool CanBeUsed()
    {
        if (playerStats == null)
            playerStats = FindFirstObjectByType<Player_Stats>();

        if(playerStats.CanApplyBuffOf(source))
            return true;
        else
        {
            Debug.Log("Same Buff cannot be applied twice.");
            return false;
        }

    }
    public override void ExecuteEffect()
    {
        playerStats.ApplyBuff(buffToApply, duration, source);

    }


}
