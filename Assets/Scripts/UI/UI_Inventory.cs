using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Inventory : MonoBehaviour
{
    private UI_ItemSlot[] uiItemSlots;
    private Inventory_Player inventory;
    private UI_EquipSlot[] uiEquipSlots;

    [SerializeField] private Transform uiItemSlotParent;
    [SerializeField] private Transform uiEquipSlotParent;

   

    private void Awake()
    {
        uiItemSlots = uiItemSlotParent.GetComponentsInChildren<UI_ItemSlot>();
        uiEquipSlots = uiEquipSlotParent.GetComponentsInChildren<UI_EquipSlot>();
        inventory = FindFirstObjectByType<Inventory_Player>();

        inventory.OnInventoryChange += UpdateUI;

        UpdateUI();
       
    }
  
    private void UpdateUI()
    {
        UpdateInventorySlots();
        UpdateEquipmentSlots();
    }

    private void UpdateEquipmentSlots()
    {
        List<Inventory_EquipmentSlot> playerEquipList = inventory.equipmentList;
      
        for (int i = 0; i < uiEquipSlots.Length; i++)
        {
            var playerEquipSlot  = playerEquipList[i];

            if (playerEquipSlot.HasItem() == false)
                uiEquipSlots[i].UpdateSlot(null);
            else
                uiEquipSlots[i].UpdateSlot(playerEquipSlot.equipmentItem);
        }

    }

    private void UpdateInventorySlots()
    {
        List<Inventory_Item> itemLists = inventory.itemList;

        for (int i = 0; i < uiItemSlots.Length; i++)
        {
            if(i < itemLists.Count)
            {
                uiItemSlots[i].UpdateSlot(itemLists[i]);
            }
            else
            {
                uiItemSlots[i].UpdateSlot(null);
            }

        }
    }

}
