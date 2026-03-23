using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item list", fileName = "List of items - ")]
public class ItemListDataSo : ScriptableObject
{
    public ItemDataSo[] itemList;

    public ItemDataSo GetItemData(string saveID)
    {
        return itemList.FirstOrDefault(item => item != null && item.saveID == saveID);
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-fill with all ItemDataSo")]
    public void CollectItemsData()
    {
        string[] guids = AssetDatabase.FindAssets("t:ItemDataSo");

        itemList = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<ItemDataSo>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item != null)
            .ToArray();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif

}
