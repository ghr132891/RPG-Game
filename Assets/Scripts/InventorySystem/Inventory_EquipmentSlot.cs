using System;
using UnityEngine;

[Serializable]
public class Inventory_EquipmentSlot 
{
    public ItemType slotType;
    public Inventory_Item equipmentItem;

    public bool HasItem() => equipmentItem != null && equipmentItem.itemData != null;

}
