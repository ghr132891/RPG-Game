using UnityEngine;

public class UI_Craft : MonoBehaviour
{
    [SerializeField] private UI_ItemSlotParent inventoryParent;
    private Inventory_Player inventory;

    private UI_CraftPreview craftPreviewUI;
    private UI_CraftListButton[] craftListButtons;
    private UI_CraftSlot[] craftSlots;

    private void Awake()
    {
        
    }

    public void SetupCraftUI(Inventory_Storage storage)
    {
        inventory = storage.playerInventory;
        inventory.OnInventoryChange += UpdateUI;
        UpdateUI();

        craftPreviewUI = GetComponentInChildren<UI_CraftPreview>();
        craftPreviewUI.SetupCraftPreview(storage);
        SetupCraftListButtons();
    }

    private void SetupCraftListButtons()
    {
        craftSlots = GetComponentsInChildren<UI_CraftSlot>();
        craftListButtons = GetComponentsInChildren<UI_CraftListButton>();

        foreach(var slot in craftSlots)
            slot.gameObject.SetActive(false);

        foreach(var slot in craftListButtons)
            slot.SetCraftSlots(craftSlots);

    }

    private void UpdateUI() => inventoryParent.UpdateSlots(inventory.itemList);
}
