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

    public SerializeableDictionary<string, bool> unlockedCheckPoints; // checkPoint ID  ，unlocked status
    public SerializeableDictionary<string, Vector3> inScenePortals;// scene Name,portal Position 

    public string portalDestinationSceneName;
    public bool returningFromTown;

    public string lastScenePlayed;
    public Vector3 lastPlayerPosition;
    public string lastCheckPointID; // 【新增】：记录最后一个激活的存档点ID

    public GameData()
    {
        inventory = new SerializeableDictionary<string, int>();
        storageItems = new SerializeableDictionary<string, int>();
        storageMaterials = new SerializeableDictionary<string, int>();

        equipedItems = new SerializeableDictionary<string, ItemType>();

        skillTreeUI = new SerializeableDictionary<string, bool>();
        skillUpgrades = new SerializeableDictionary<SkillType, SkillUpgradeType>();
        unlockedCheckPoints = new SerializeableDictionary<string, bool>();
        inScenePortals = new SerializeableDictionary<string, Vector3>();
        lastCheckPointID = string.Empty;
    }

}
