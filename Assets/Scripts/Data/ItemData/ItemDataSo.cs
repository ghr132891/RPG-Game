
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Material item", fileName = "Material data - ")]
public class ItemDataSo : ScriptableObject
{
    public string saveID { get; private set; }

    [Header("Merchant Details")]
    [Range(0, 10000)]
    public float itemPrice = 100;
    public int minStackSizeAtShop = 1;
    public int maxStackSizeAtShop = 1;

    [Header("Drop Details")]
    [Range(0, 1000)]
    public int itemRarity = 100;
    [Range(0, 100)]
    public float dropChance;
    [Range(0, 100)]
    public float maxDropChance = 65f;



    [Header("Craft Details")]
    public Inventory_Item[] craftRecipe;

    [Header("Item Details")]
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public int maxStackSize = 1;

    [Header("Item effect")]
    public ItemEffect_DataSo itemEffect;

    private void OnValidate()
    {
        dropChance = GetDropChance();

#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(this);
        saveID= AssetDatabase.AssetPathToGUID(path);
#endif
    }

    public float GetDropChance()
    { 
        float maxRarity = 1000;
        float chance = (maxRarity - itemRarity) / maxRarity * 100;

        return chance > 0 ? Mathf.Min(chance, maxDropChance) : 0.1f;
    }


}
