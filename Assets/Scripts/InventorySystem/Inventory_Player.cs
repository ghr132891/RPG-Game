using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : Inventory_Base
{
    public event Action<int> OnQuickItemUsed;

    public List<Inventory_EquipmentSlot> equipmentList;
    public Inventory_Storage storage { get; private set; }

    [Header("Quick Item Slots")]
    public Inventory_Item[] quickItems = new Inventory_Item[2];

    [Header("Gold Info")]
    public float gold = 10000;

    protected override void Awake()
    {
        base.Awake();
        storage = FindFirstObjectByType<Inventory_Storage>();
    }

    public void SetQuickItemSlot(int slotNumber, Inventory_Item itemToSlot)
    {
        quickItems[slotNumber - 1] = itemToSlot;
        TriggerUpdateUI();
    }

    public void TryUseQuickItemSlot(int passedSlotNumber)
    {
        int slotNumber = passedSlotNumber - 1;
        var itemToUsed = quickItems[slotNumber];

        if (itemToUsed == null)
            return;

        TryUseItem(itemToUsed);

        if (FindItem(itemToUsed) == null)
        {
            quickItems[slotNumber] = FindSameItem(itemToUsed);
        }

        TriggerUpdateUI();
        OnQuickItemUsed?.Invoke(slotNumber);
    }

    public void TryEquipItem(Inventory_Item item)
    {
        var inventoryITem = FindItem(item);
        var matchingSlots = equipmentList.FindAll(slot => slot.slotType == item.itemData.itemType);

        foreach (var slot in matchingSlots)
        {
            if (slot.HasItem() == false)
            {
                EquipItem(inventoryITem, slot);
                return;
            }
        }

        var slotToReplace = matchingSlots[0];
        var itemToUnequip = slotToReplace.equipmentItem;

        UnequipItem(itemToUnequip, slotToReplace != null);
        EquipItem(inventoryITem, slotToReplace);

    }
    private void EquipItem(Inventory_Item itemToEquip, Inventory_EquipmentSlot slot)
    {
        float savedHealthPercent = player.health.GetHealthPercent();

        slot.equipmentItem = itemToEquip;
        slot.equipmentItem.Addmodifiers(player.stats);
        slot.equipmentItem.AddItemEffect(player);


        player.health.SetHealthToPercent(savedHealthPercent);
        RemoveOneItem(itemToEquip);
    }

    public void UnequipItem(Inventory_Item itemToUnequip, bool replacingItem = false)
    {
        if (canAddItem(itemToUnequip) == false && replacingItem == false)
        {
            Debug.Log("No, Space.");
            return;
        }

        float savedHealthPercent = player.health.GetHealthPercent();

        var slotToUnequip = equipmentList.Find(slot => slot.equipmentItem == itemToUnequip);

        if (slotToUnequip != null)
            slotToUnequip.equipmentItem = null;

        itemToUnequip.RemoveModifiers(player.stats);
        itemToUnequip.RemoveItemEffect();



        player.health.SetHealthToPercent(savedHealthPercent);
        AddItem(itemToUnequip);
    }

    public override void SaveData(ref GameData gameData)
    {
        gameData.gold = gold;
        gameData.healthPercent = player.health.GetHealthPercent();
        gameData.inventory.Clear();
        gameData.equipedItems.Clear();

        foreach (var entryItem in itemList)
        {
            if (entryItem != null && entryItem.itemData != null)
            {
                string saveID = entryItem.itemData.saveID;

                if (gameData.inventory.ContainsKey(saveID) == false)
                    gameData.inventory[saveID] = 0;

                gameData.inventory[saveID] += entryItem.stackSize;
            }
        }

        foreach (var slot in equipmentList)
        {
            if (slot.HasItem())
                gameData.equipedItems[slot.equipmentItem.itemData.saveID] = slot.slotType;

        }
    }

    public override void LoadData(GameData gameData)
    {
        gold = gameData.gold;

        foreach (var entryItem in gameData.inventory)
        {
            string saveID = entryItem.Key;
            int stackSize = entryItem.Value;

            ItemDataSo itemData = itemDataBase.GetItemData(saveID);

            if (itemData == null)
            {
                Debug.Log("Item Not found: " + itemData.saveID);
                continue;
            }

            for (int i = 0; i < stackSize; i++)
            {
                Inventory_Item itemToload = new Inventory_Item(itemData);
                AddItem(itemToload);
            }

        }

        foreach (var entryItem in gameData.equipedItems)
        {

            string saveID = entryItem.Key;
            ItemType loadedSlotType = entryItem.Value;

            ItemDataSo itemData = itemDataBase.GetItemData(saveID);
            Inventory_Item itemToLoad = new Inventory_Item(itemData);

            var slot = equipmentList.Find(slot => slot.slotType == loadedSlotType && slot.HasItem() == false);

            slot.equipmentItem = itemToLoad;
            slot.equipmentItem.Addmodifiers(player.stats);
            slot.equipmentItem.AddItemEffect(player);
        }

        StartCoroutine(ApplyHealthBar(gameData.healthPercent));// set healthbar.
        TriggerUpdateUI();
    }

    private IEnumerator ApplyHealthBar(float percent)
    {
        yield return null; // next frame set health.
        player.health.SetHealthToPercent(percent);
    }

}
