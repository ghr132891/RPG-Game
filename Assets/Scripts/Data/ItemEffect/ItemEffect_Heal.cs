using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/Heal Effect", fileName = "Item Effect data - ")]
public class ItemEffect_Heal : ItemEffect_DataSo
{
    [SerializeField] private float healPercent = 1f;
    public override void ExecuteEffect()
    {
        base.ExecuteEffect();
        Player player = FindFirstObjectByType<Player>();

        float healAmount = player.stats.GetMaxHealth() * healPercent;
        player.health.IncreaseHealth(healAmount);
    }

}
