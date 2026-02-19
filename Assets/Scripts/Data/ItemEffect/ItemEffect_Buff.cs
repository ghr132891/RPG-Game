using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/Buff Effect", fileName = "Item Effect data - Buff")]
public class ItemEffect_Buff : ItemEffect_DataSo
{
    [SerializeField] private BuffEffectData[] buffToApply;
    [SerializeField] private float duration;
    [SerializeField] private string source = Guid.NewGuid().ToString();


    public override bool CanBeUsed(Player player)
    {


        if (player.stats.CanApplyBuffOf(source))
        {
            this.player = player;
            return true;
        }
        else
        {
            Debug.Log("Same Buff cannot be applied twice.");
            return false;
        }

    }
    public override void ExecuteEffect()
    {
        player.stats.ApplyBuff(buffToApply, duration, source);

    }


}
