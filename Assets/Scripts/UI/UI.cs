using UnityEngine;

public class UI : MonoBehaviour
{
    #region UI Compoents
    public UI_SkillToolTip skillToolTip {  get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_StatToolTip statToolTip { get; private set; }
    public UI_Inventory inventoryUI { get; private set; }

    public UI_SkillTree skillTreeUI { get; private set; }
    public UI_Storage storageUI { get; private set; }
    public UI_Craft craftUI { get; private set; }
    public UI_Merchant merchantUI { get; private set; }
    public UI_InGame inGameUI { get; private set; }
    #endregion

    private bool skillTreeEnabled;
    private bool inventoryEnable;

    private void Awake()
    {
        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        skillTreeUI = GetComponentInChildren<UI_SkillTree>(true);
        statToolTip = GetComponentInChildren<UI_StatToolTip>();
        inventoryUI = GetComponentInChildren<UI_Inventory>(true);
        storageUI = GetComponentInChildren<UI_Storage>(true);
        craftUI = GetComponentInChildren<UI_Craft>(true);
        merchantUI = GetComponentInChildren<UI_Merchant>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        

        inventoryEnable = inventoryUI.gameObject.activeSelf;
        skillTreeEnabled = skillTreeUI.gameObject.activeSelf;
    }

    private void Start()
    {
        skillTreeUI.UnlockDefaultSkills();
    }

    public void SwitchOffAllToolTips()
    {
        itemToolTip.ShowToolTip(false, null);
        skillToolTip.ShowToolTip(false, null);
        statToolTip.ShowToolTip(false, null);
    }
    public void ToggleSkillTreeUI()
    {
        skillTreeEnabled = !skillTreeEnabled;
        skillTreeUI.gameObject.SetActive(skillTreeEnabled);
        skillToolTip.ShowToolTip(false, null);

    }

    public void ToggleInventoryUI()
    {
        inventoryEnable = !inventoryEnable;
        inventoryUI.gameObject.SetActive(inventoryEnable);
        statToolTip.ShowToolTip(false, null);
        itemToolTip.ShowToolTip(false, null);

    }
}
