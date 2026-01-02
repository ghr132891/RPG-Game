using UnityEngine;

public class Object_ItemPickUp : MonoBehaviour
{
    private SpriteRenderer sr;
    [SerializeField] private ItemDataSo itemData;

    private Inventory_Item itemToAdd;
    private Inventory_Base inventory;


    private void Awake()
    {
        itemToAdd = new Inventory_Item(itemData);

    }

    private void OnValidate()
    {
        if (itemData == null)
            return;

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = itemData.itemIcon;
        gameObject.name = "Object_ItemPickUp - " + itemData.itemName;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        inventory = collision.GetComponent<Inventory_Base>();

        if (inventory == null)
            return;

        bool canAddItem = inventory.canAddItem() ||inventory.StackableItem(itemToAdd) != null;

        if (canAddItem)
        {
            inventory.AddItem(itemToAdd);
            Destroy(gameObject);
        }
    }

}
