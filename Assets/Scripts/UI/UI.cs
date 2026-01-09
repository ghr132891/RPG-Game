using UnityEngine;

public class UI : MonoBehaviour
{
    public UI_SkillToolTip skillToolTip {  get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_StatToolTip statToolTip { get; private set; }
    public UI_Inventory inventoryUI { get; private set; }

    public UI_SkillTree skillTreeUI { get; private set; }

    private bool skillTreeEnabled;
    private bool inventoryEnable;

    private void Awake()
    {
        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        skillTreeUI = GetComponentInChildren<UI_SkillTree>(true);
        statToolTip = GetComponentInChildren<UI_StatToolTip>();
        inventoryUI = GetComponentInChildren<UI_Inventory>(true);

        inventoryEnable = inventoryUI.gameObject.activeSelf;
        skillTreeEnabled = skillTreeUI.gameObject.activeSelf;
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
