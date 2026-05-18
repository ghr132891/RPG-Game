using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI instance;

    [SerializeField] private GameObject[] uiElements;
    public bool alternativeInput { get; private set; }
    private PlayerInputSet input;


    #region UI Compoents
    public UI_SkillToolTip skillToolTip { get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_StatToolTip statToolTip { get; private set; }
    public UI_Inventory inventoryUI { get; private set; }

    public UI_SkillTree skillTreeUI { get; private set; }
    public UI_Storage storageUI { get; private set; }
    public UI_Craft craftUI { get; private set; }
    public UI_Merchant merchantUI { get; private set; }
    public UI_InGame inGameUI { get; private set; }
    public UI_Options optionsUI { get; private set; }
    public UI_DeathScreen deathScreemUI { get; private set; }
    public UI_FadeScreen fadeScreenUI { get; private set; }
    #endregion

    private bool skillTreeEnabled;
    private bool inventoryEnable;

    private void Awake()
    {
        instance = this;

        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        skillTreeUI = GetComponentInChildren<UI_SkillTree>(true);
        statToolTip = GetComponentInChildren<UI_StatToolTip>();
        inventoryUI = GetComponentInChildren<UI_Inventory>(true);
        storageUI = GetComponentInChildren<UI_Storage>(true);
        craftUI = GetComponentInChildren<UI_Craft>(true);
        merchantUI = GetComponentInChildren<UI_Merchant>(true);
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        optionsUI = GetComponentInChildren<UI_Options>(true);
        deathScreemUI = GetComponentInChildren<UI_DeathScreen>(true);
        fadeScreenUI = GetComponentInChildren<UI_FadeScreen>(true); 


        inventoryEnable = inventoryUI.gameObject.activeSelf;
        skillTreeEnabled = skillTreeUI.gameObject.activeSelf;
    }

    private void Start()
    {
        skillTreeUI.UnlockDefaultSkills();
    }

    // ================= 新增：订阅玩家死亡事件 =================
    private void OnEnable()
    {
        // 只要 Player 广播了 OnPlayerDeadth，立刻无条件打开死亡界面
        Player.OnPlayerDeadth += OpenDeathScreenUI;
    }

    private void OnDisable()
    {
        // 移除监听，防止报错
        Player.OnPlayerDeadth -= OpenDeathScreenUI;
    }


    private void StopPlayerControls(bool stopControls)
    {
        if (stopControls)
            input.Player.Disable();
        else
            input.Player.Enable();

    }

    private void StopPlayerControlsIfNeeded()
    {
        foreach (var element in uiElements)
        {
            if (element.activeSelf)
            {
                StopPlayerControls(true);
                return;
            }
        }
        StopPlayerControls(false);
    }
    public void SetupControlUI(PlayerInputSet inputSet)
    {
        input = inputSet;

        input.UI.SkillTreeUI.performed += ctx => ToggleSkillTreeUI();
        input.UI.InventoryUI.performed += ctx => ToggleInventoryUI();

        input.UI.AlternativeInput.performed += ctx => alternativeInput = true;
        input.UI.AlternativeInput.canceled += ctx => alternativeInput = false;

        input.UI.OptionsUI.performed += ctx =>
        {
            foreach (var element in uiElements)
            {
                if (element.activeSelf)
                {
                    Time.timeScale = 1;
                    SwtichToInGameUI();
                    return;
                }
            }

            Time.timeScale = 0;
            skillTreeEnabled = false;
            OpenOptionsUI();
        };

    }

    public void OpenDeathScreenUI()
    {
        SwitchTo(deathScreemUI.gameObject);
        input.Disable();//keyborad input should be disabled .
    }

    public void OpenOptionsUI()
    {
        HideAllToolTips();
        StopPlayerControls(true);
        SwitchTo(optionsUI.gameObject);
    }

    private void SwtichToInGameUI()
    {  
        HideAllToolTips();
        StopPlayerControls(false);
        SwitchTo(inGameUI.gameObject);

        skillTreeEnabled = false;
        inventoryEnable = false;
    }

    private void SwitchTo(GameObject objectToSwitchOn)
    {
        foreach(var element in uiElements)
            element.gameObject.SetActive(false);

        objectToSwitchOn.SetActive(true);
    }
    public void ToggleSkillTreeUI()
    {
        skillTreeUI.transform.SetAsLastSibling();
        SetTooltipsAsLastSibling();
        fadeScreenUI.transform.SetAsLastSibling();

        skillTreeEnabled = !skillTreeEnabled;
        skillTreeUI.gameObject.SetActive(skillTreeEnabled);
        HideAllToolTips();

        StopPlayerControlsIfNeeded();
    }

    public void ToggleInventoryUI()
    {
        inventoryUI.transform.SetAsLastSibling();
        SetTooltipsAsLastSibling();
        fadeScreenUI.transform.SetAsLastSibling();

        inventoryEnable = !inventoryEnable;
        inventoryUI.gameObject.SetActive(inventoryEnable);
        HideAllToolTips();

        StopPlayerControlsIfNeeded();
    }

    public void OpenStorageUI(bool openStorageUI)
    {
        storageUI.gameObject.SetActive(openStorageUI);
        StopPlayerControls(openStorageUI);

        if (openStorageUI == false)
        {
            craftUI.gameObject.SetActive(false);
            HideAllToolTips();
        }
    }

    public void OpenMerchantUI(bool openMerchantUI)
    {
        merchantUI.gameObject.SetActive(openMerchantUI);
        StopPlayerControls(openMerchantUI);

        if (openMerchantUI == false)
            HideAllToolTips();
    }

    public void HideAllToolTips()
    {
        itemToolTip.ShowToolTip(false, null);
        skillToolTip.ShowToolTip(false, null);
        statToolTip.ShowToolTip(false, null);
    }

    private void SetTooltipsAsLastSibling()
    {
        itemToolTip.transform.SetAsLastSibling();
        skillToolTip.transform.SetAsLastSibling();
        statToolTip.transform.SetAsLastSibling();
    }

}
