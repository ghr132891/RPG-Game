using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour,ISaveable
{
    protected Player player;
    public event Action OnInventoryChange;
    [SerializeField] protected ItemListDataSo itemDataBase;

    public int maxInventorySize = 15;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    protected virtual void Awake()
    {
        player = GetComponent<Player>();
    }
    public void TryUseItem(Inventory_Item ItemToUse)
    {
        Inventory_Item consumable = itemList.Find(item => item == ItemToUse);

        if (consumable == null)
            return;

        if (consumable.itemEffect.CanBeUsed(player) == false)
            return;

        consumable.itemEffect.ExecuteEffect();

        if (consumable.stackSize > 1)
            consumable.RemoveStack();
        else
            RemoveOneItem(consumable);

        OnInventoryChange?.Invoke();

    }
    public bool canAddItem(Inventory_Item itemToAdd)
    {
        bool hasStackable = FindStackable(itemToAdd) != null;
        return hasStackable || itemList.Count < maxInventorySize;
    }

    public Inventory_Item FindStackable(Inventory_Item itemToAdd)
    {
        return itemList.Find(item => item.itemData == itemToAdd.itemData && item.canAddStack());


    }

    public virtual void AddItem(Inventory_Item itemToAdd)
    {
        var existingStackable = FindStackable(itemToAdd);

        if (existingStackable != null)
            existingStackable.AddStack();
        else
            itemList.Add(itemToAdd);

        OnInventoryChange?.Invoke();
    }

    public void RemoveOneItem(Inventory_Item itemToRemove)
    {
        Inventory_Item itemInInventory = itemList.Find(item => item == itemToRemove);

        if (itemInInventory.stackSize > 1)
            itemInInventory.RemoveStack();
        else
            itemList.Remove(itemToRemove);

        OnInventoryChange?.Invoke();
    }

    public void RemoveFullStack(Inventory_Item itemToRemove)
    {
        for (int i = 0; i < itemToRemove.stackSize; i++)
        {
            RemoveOneItem(itemToRemove);
        }
    }

    public Inventory_Item FindItem(Inventory_Item itemToFind)
    {
        return itemList.Find(item => item == itemToFind);
    }

    public Inventory_Item FindSameItem(Inventory_Item itemToFind)
    {
        return itemList.Find(item => item.itemData == itemToFind.itemData);
    }
    public void TriggerUpdateUI() => OnInventoryChange?.Invoke();

    public virtual void LoadData(GameData gameData)
    {
        
    }

    public virtual void SaveData(ref GameData gameData)
    {
        
    }
}
