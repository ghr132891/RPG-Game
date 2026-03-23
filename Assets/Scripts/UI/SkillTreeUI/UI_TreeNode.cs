using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private RectTransform rect;
    private UI_SkillTree skillTree;
    private UI_TreeConnectHandler treeConnectHandler;

    [Header("Unlock Details")]
    public UI_TreeNode[] neededNodes;
    public UI_TreeNode[] conflictNodes;
    public bool isLocked;
    public bool isUnlocked;


    [Header("Skill Details")]
    public Skill_DataSO skillData;
    [SerializeField] public string skillName;
    [SerializeField] private Image skillIcon;
    [SerializeField] private int skillCost;
    [SerializeField] private string lockedColorHex = "#4E4E4E";
    private Color lastColor;



    private void Start()
    {
        if(isUnlocked == false)
            UpdateIconColor(GetColorByHex(lockedColorHex));

        UnlockDefaultSkills();
         
    }

    public void UnlockDefaultSkills()
    {
        GetNeededComponents();

        if (skillData.unlockedBydefault)
            UnLock();
    }
    private void GetNeededComponents()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        skillTree = GetComponentInParent<UI_SkillTree>(true);
        treeConnectHandler = GetComponent<UI_TreeConnectHandler>();
    }

    public void Refund()
    {
        if (isUnlocked == false || skillData.unlockedBydefault)
            return;

        isUnlocked = false;
        isLocked = false;

        UpdateIconColor(GetColorByHex(lockedColorHex));

        skillTree.AddSkillPoints(skillData.cost);
        treeConnectHandler.UnlockConnectionImage(false);

    }

    private void UnLock()
    {
        if (isUnlocked)
        {
            Debug.Log("Skill is already unlocked");
            return;

        }

        isUnlocked = true;
        UpdateIconColor(Color.white);
        LockConflictNodes();
        skillTree.RemoveSkillPoints(skillData.cost);
        treeConnectHandler.UnlockConnectionImage(true);

        skillTree.skillManager.GetSkillByType(skillData.skillType).SetSkillUpgrade(skillData);

    }

    public void UnlockWithSaveData()
    {
        isUnlocked = true;
        UpdateIconColor(Color.white);
        LockConflictNodes();

        treeConnectHandler.UnlockConnectionImage(true);


    }
    private bool CanBeUnlocked()
    {
        if (isUnlocked || isLocked)
            return false;

        if (skillTree.EnoughSkillPoints(skillData.cost) == false)
            return false;

        foreach (var node in neededNodes)
        {

            if (node.isUnlocked == false)
                return false;
        }

        foreach (var node in conflictNodes)
        {

            if (node.isUnlocked)
                return false;
        }


        return true;

    }

    private void LockConflictNodes()
    {
        foreach (var node in conflictNodes)
        {

            node.isLocked = true;
            node.LockChildNodes();
        }
    }

    public void LockChildNodes()
    {
        isLocked = true;

        foreach (var node in treeConnectHandler.GetChildNodes())
        {
            node.LockChildNodes();
        }
    }



    private void UpdateIconColor(Color color)
    {
        if (skillIcon == null)
            return;

        lastColor = skillIcon.color;
        skillIcon.color = color;

    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanBeUnlocked())
            UnLock();
        else if (isLocked)
            ui.skillToolTip.LockedSkillEffect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(true, rect,skillData, this);

        if (isUnlocked || isLocked)
            return;

        ToggleNodeHighLight(true);




    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(false, rect);
        ui.skillToolTip.StopLockedSkillEffect();

        if (isUnlocked || isLocked)
            return;

        ToggleNodeHighLight(false);
    }

    private void ToggleNodeHighLight(bool highlight)
    {
        Color highLightColor = Color.white * .8f; highLightColor.a = 1f;
        Color colorToApply = highlight ? highLightColor : lastColor;

        UpdateIconColor(colorToApply);


    }

    private Color GetColorByHex(string hexNumber)
    {
        ColorUtility.TryParseHtmlString(hexNumber, out Color color);

        return color;

    }
    private void OnDisable()
    {
        if (isLocked)
            UpdateIconColor(GetColorByHex(lockedColorHex));

        if (isUnlocked)
            UpdateIconColor(Color.white);


    }
    private void OnValidate()
    {
        if (skillData == null)
            return;

        skillName = skillData.skillName;
        skillIcon.sprite = skillData.icon;
        skillCost = skillData.cost;
        gameObject.name = "UI TreeNode - " + skillName;

    }

}
