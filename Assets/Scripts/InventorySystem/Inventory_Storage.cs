using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory_Storage : Inventory_Base
{
    public Inventory_Player playerInventory { get; private set; }
    public List<Inventory_Item> materialStash;

    public void CraftItem(Inventory_Item itemToCraft)
    {
        ConsumeMaterials(itemToCraft);
        playerInventory.AddItem(itemToCraft);
    }


    public bool CanCraftItem(Inventory_Item itemToCraft)
    {
        return HasEnoughMaterials(itemToCraft) && playerInventory.canAddItem(itemToCraft);
    }


    private void ConsumeMaterials(Inventory_Item itemToCraft)
    {
        foreach (var requiredItem in itemToCraft.itemData.craftRecipe)
        {
            int amountToConsume = requiredItem.stackSize;

            amountToConsume = amountToConsume - ConsumedMaterialsAmount(playerInventory.itemList, requiredItem);

            if (amountToConsume > 0)
                amountToConsume = amountToConsume - ConsumedMaterialsAmount(itemList, requiredItem);

            if (amountToConsume > 0)
                amountToConsume = amountToConsume - ConsumedMaterialsAmount(materialStash, requiredItem);
        }

    }


    private int ConsumedMaterialsAmount(List<Inventory_Item> itemList, Inventory_Item neededItem)
    {
        int amountNeeded = neededItem.stackSize;
        int consumedAmount = 0;
        List<Inventory_Item> itemsToremove = new();

        foreach (var item in itemList)
        {
            if (item.itemData != neededItem.itemData)
                continue;

            int removeAmount = Mathf.Min(item.stackSize, amountNeeded - consumedAmount);
            item.stackSize = item.stackSize - removeAmount;
            consumedAmount = consumedAmount + removeAmount;

            if (item.stackSize <= 0)
                itemsToremove.Add(item);

            if (consumedAmount >= amountNeeded)
                break;

        }

        foreach (Inventory_Item item in itemsToremove)
            itemList.Remove(item);

        return consumedAmount;

    }

    private bool HasEnoughMaterials(Inventory_Item itemToCraft)
    {
        foreach (var requiredMaterial in itemToCraft.itemData.craftRecipe)
        {
            if (GetAvailableAmountOf(requiredMaterial.itemData) < requiredMaterial.stackSize)
                return false;
        }

        return true;
    }

    public int GetAvailableAmountOf(ItemDataSo requiredItem)
    {
        int amount = 0;

        foreach (var item in playerInventory.itemList)
        {
            if (item.itemData == requiredItem)
                amount = amount + item.stackSize;
        }

        foreach (var item in itemList)
        {
            if (item.itemData == requiredItem)
                amount = amount + item.stackSize;
        }

        foreach (var item in materialStash)
        {
            if (item.itemData == requiredItem)
                amount = amount + item.stackSize;
        }

        return amount;
    }

    public void AddMaterialToStash(Inventory_Item itemToAdd)
    {
        var stackableItem = StackableInStash(itemToAdd);

        if (stackableItem != null)
            stackableItem.AddStack();
        else
        {
            var newItemToAdd = new Inventory_Item(itemToAdd.itemData);
            materialStash.Add(newItemToAdd);
        }

        TriggerUpdateUI();
        materialStash = materialStash.OrderBy(item => item.itemData.name).ToList();
    }

    public Inventory_Item StackableInStash(Inventory_Item itemToAdd)
    {
        return materialStash.Find(item => item.itemData == itemToAdd.itemData && item.canAddStack());
    }

    public void SetInventory(Inventory_Player inventory) => this.playerInventory = inventory;

    public void FromPlayerToStorage(Inventory_Item item, bool transferFullStack)
    {
        int transferAmount = transferFullStack ? item.stackSize : 1;

        for (int i = 0; i < transferAmount; i++)
        {
            if (canAddItem(item))
            {
                var itemToAdd = new Inventory_Item(item.itemData);

                playerInventory.RemoveOneItem(item);
                AddItem(itemToAdd);
            }
        }
        TriggerUpdateUI();
    }

    public void FromStorageToPlayer(Inventory_Item item, bool transferFullStack)
    {
        int transferAmount = transferFullStack ? item.stackSize : 1;

        for (int i = 0; i < transferAmount; i++)
        {
            if (playerInventory.canAddItem(item))
            {
                var itemToAdd = new Inventory_Item(item.itemData);
                RemoveOneItem(item);
                playerInventory.AddItem(itemToAdd);
            }
        }

        TriggerUpdateUI();
    }

    public override void SaveData(ref GameData gameData)
    {
        base.SaveData(ref gameData);

        gameData.storageItems.Clear();

        foreach (var item in itemList)
        {
            if(item != null && item.itemData != null)
            {
                string saveID = item.itemData.saveID;

                if (gameData.storageItems.ContainsKey(saveID) == false)
                    gameData.storageItems[saveID] = 0;

                gameData.storageItems[saveID] +=item.stackSize;
            }
        }

        gameData.storageMaterials.Clear();

        foreach (var entryItem in materialStash)
        {
            if(entryItem != null && entryItem.itemData != null)
            {
                string saveID = entryItem.itemData.saveID;

                if (gameData.storageMaterials.ContainsKey(saveID) == false)
                    gameData.storageMaterials[saveID] = 0;

                gameData.storageMaterials[saveID] += entryItem.stackSize;

            }

        }
    }

    public override void LoadData(GameData gameData)
    {
        itemList.Clear();
        materialStash.Clear();

        foreach (var entryItem in gameData.storageItems)
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

        foreach (var entryItem in gameData.storageMaterials)
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
                AddMaterialToStash(itemToload);
            }

        }

        TriggerUpdateUI();

    }

}
