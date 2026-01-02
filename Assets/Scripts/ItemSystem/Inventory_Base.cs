using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour
{
    public event Action OnInventoryChange;

    public int maxInventorySize = 15;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    protected virtual void Awake()
    {
        
    }
    public bool canAddItem() => itemList.Count < maxInventorySize;
    public bool canAddToStack(Inventory_Item itemToAdd)
    {
        List<Inventory_Item> stackableItems = itemList.FindAll(item => item.itemData == itemToAdd.itemData);

        foreach(var stack in stackableItems)
        {
            if(stack.canAddStack())
                return true;
        }

        return false;
    }

    public Inventory_Item StackableItem(Inventory_Item itemToAdd)
    {
        List<Inventory_Item> stackableItems = itemList.FindAll(item => item.itemData == itemToAdd.itemData);
        
        foreach(var stackableItem in stackableItems)
        {
            if(stackableItem.canAddStack())
                return stackableItem;
        }
        return null;
    }

    public virtual void AddItem(Inventory_Item itemToAdd)
    {
        var existingStackable = StackableItem(itemToAdd);

        if (existingStackable != null)
            existingStackable.AddStack();
        else
            itemList.Add(itemToAdd);

        OnInventoryChange?.Invoke();
    }

    public void RemoveItem(Inventory_Item itemToRemove)
    {
        itemList.Remove(FindItem(itemToRemove.itemData));
        OnInventoryChange?.Invoke();
    }

    public Inventory_Item FindItem(ItemDataSo itemData)
    {
        return itemList.Find(item => item.itemData == itemData );
    }

}
