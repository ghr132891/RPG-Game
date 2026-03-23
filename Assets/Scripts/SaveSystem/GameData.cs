using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public float gold;
    public float healthPercent;

    public List<Inventory_Item> itemList;
    public SerializeableDictionary<string, int> inventory;// itemSaveID , stackSize
    public SerializeableDictionary<string, int> storageItems;
    public SerializeableDictionary<string, int> storageMaterials;

    public SerializeableDictionary<string, ItemType> equipedItems;// itemSaveID, slotType

    public SerializeableDictionary<string, bool> skillTreeUI; // skill name  ,unlock stats
    public int skillPoints;
    public SerializeableDictionary<SkillType, SkillUpgradeType> skillUpgrades; // skill type, upgrade type

    public Vector3 savedCheckPoint;
    public GameData()
    {
        inventory = new SerializeableDictionary<string, int>();
        storageItems = new SerializeableDictionary<string, int>();
        storageMaterials = new SerializeableDictionary<string, int>();

        equipedItems = new SerializeableDictionary<string, ItemType>();

        skillTreeUI = new SerializeableDictionary<string, bool>();
        skillUpgrades = new SerializeableDictionary<SkillType, SkillUpgradeType>();

    }

}
