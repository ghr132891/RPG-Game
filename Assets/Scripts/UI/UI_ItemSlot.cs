using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour,IPointerDownHandler,IPointerEnterHandler,IPointerExitHandler
{
    public Inventory_Item itemInSLot { get; private set; }
    protected Inventory_Player inventory;
    protected UI ui;
    protected RectTransform rect;

    [Header("UI Slot SetUp")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemStackSize;

    protected void Awake()
    {
        rect = GetComponent<RectTransform>();
        ui= GetComponentInParent<UI>();
        inventory = FindAnyObjectByType<Inventory_Player>();
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if(inventory == null || itemInSLot.itemData.itemType == ItemType.Material)
            return;
        if (itemInSLot.itemData.itemType == ItemType.Consumable)
        {
            if(itemInSLot.itemEffect.CanBeUsed() == false)
                return;

            inventory.TryUseItem(itemInSLot);
        }
        else
            inventory.TryEquipItem(itemInSLot);

        if (inventory == null)
            ui.itemToolTip.ShowToolTip(false, null);
    }
    public void UpdateSlot(Inventory_Item item)
    {
        itemInSLot = item;

        if (itemInSLot == null)
        {
            itemStackSize.text = "";
            itemIcon.color = Color.clear;
            return;
        }

        Color color = Color.white; color.a = .9f;
        itemIcon.color = color;
        itemIcon.sprite = itemInSLot.itemData.itemIcon;
        itemStackSize.text = item.stackSize > 1 ? item.stackSize.ToString() : "";

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(itemInSLot == null )
            return;

        ui.itemToolTip.ShowToolTip(true,rect,itemInSLot);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.itemToolTip.ShowToolTip(false,null);
    }
}
