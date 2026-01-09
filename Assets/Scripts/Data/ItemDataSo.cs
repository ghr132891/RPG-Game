using UnityEngine;

[CreateAssetMenu(menuName ="RPG Setup/Item Data/Material item",fileName = "Material data - ")]
public class ItemDataSo : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public int maxStackSize = 1;

    [Header("Item effect")]
    public ItemEffect_DataSo itemEffect;




}
