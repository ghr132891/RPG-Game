using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/Refund All Skills", fileName = "Item Effect data - Refund All Skills")]
public class ItemEffect_RefundAllSkills : ItemEffect_DataSo
{


    public override void ExecuteEffect()
    {
        UI ui = FindFirstObjectByType<UI>();
        ui.skillTreeUI.RefundAllSkills();

    }
}
