using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Equipment item", fileName = "Equipment data - ")]
public class EquipmentDataSo : ItemDataSo
{
    [Header("Item Mpdifiers")]
    public ItemModifier[] modifiers;


}

[Serializable]
public class ItemModifier
{
    public StatType statType;
    public float value;
}
