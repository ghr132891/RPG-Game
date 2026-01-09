using System;

[Serializable]
public class Inventory_Item
{
    private string itemID;

    public ItemDataSo itemData;
    public int stackSize = 1;

    public ItemModifier[] Modifiers { get; private set; }
    public ItemEffect_DataSo itemEffect;

    public Inventory_Item(ItemDataSo itemData)
    {
        this.itemData = itemData;
        itemEffect = itemData.itemEffect;

        Modifiers = EquipmentData()?.modifiers;
        itemID = itemData.itemName +" - " +Guid.NewGuid();
    }

    public void Addmodifiers(Entity_Stats playerStats)
    {
        foreach (var mod in Modifiers)
        {
            Stat statToModify = playerStats.GetStatByType(mod.statType);
            statToModify.AddModifier(mod.value, itemID);
        }
    }

    public void RemoveModifiers(Entity_Stats playerStats)
    {
        foreach (var mod in Modifiers)
        {
            Stat statTomodify = playerStats.GetStatByType(mod.statType);
            statTomodify.RemoveModifier(itemID);
        }
    }

    private EquipmentDataSo EquipmentData()
    {
        if (itemData is EquipmentDataSo equipment)
            return equipment;
        else
            return null;
    }

    public bool canAddStack() => stackSize < itemData.maxStackSize;
    public void AddStack() => stackSize++;
    public void RemoveStack() => stackSize--;

}
