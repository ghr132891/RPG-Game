using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : Inventory_Base
{
    private Entity_Stats playerStats;
    public List<Inventory_EquipmentSlot> equipmentList;

    protected override void Awake()
    {
        base.Awake();
        playerStats = GetComponent<Entity_Stats>();
    }

    public void TryEquipItem(Inventory_Item item)
    {
        var inventoryITem = FindItem(item.itemData);
        var matchingSlots = equipmentList.FindAll(slot => slot.slotType == item.itemData.itemType); 

        foreach (var slot in matchingSlots)
        {
            if(slot.HasItem() == false) 
            {
                EquipItem(inventoryITem, slot);
                return;
            }
        }

        var slotToReplace = matchingSlots[0];
        var itemToUnequip = slotToReplace.equipmentItem;

        EquipItem(inventoryITem,slotToReplace);
        UnequipItem(itemToUnequip);

    }
    private void EquipItem(Inventory_Item itemToEquip,Inventory_EquipmentSlot slot)
    {
        slot.equipmentItem = itemToEquip;
        slot.equipmentItem.Addmodifiers(playerStats);

        RemoveItem(itemToEquip);
    }

    public void UnequipItem(Inventory_Item itemToUnequip)
    {
        if(canAddItem() == false)
        {
            Debug.Log("No, Space.");
            return;
        }

        foreach(var slot in equipmentList)
        {
            if(slot.equipmentItem == itemToUnequip)
            {
                slot.equipmentItem = null;
                
                break;
            }
        }
        itemToUnequip.RemoveModifiers(playerStats);
        AddItem(itemToUnequip);
    }
}
