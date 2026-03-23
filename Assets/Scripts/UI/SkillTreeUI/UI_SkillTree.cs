using JetBrains.Annotations;
using System.Linq;
using TMPro;
using UnityEngine;

public class UI_SkillTree : MonoBehaviour,ISaveable
{
    [SerializeField] private int skillPoints;
    [SerializeField] private TextMeshProUGUI skillPointsText;
    [SerializeField] private UI_TreeConnectHandler[] parentNodes;
    private UI_TreeNode[] allTreeNodes;
    public Player_SkillManager skillManager { get; private set; }

    public bool EnoughSkillPoints(int cost) => skillPoints >= cost;
    public void RemoveSkillPoints(int cost)
    {
        skillPoints -= cost;
        UpdateSkillPointsUI();
    }

    public void AddSkillPoints(int cost)
    {
        skillPoints += cost;
        UpdateSkillPointsUI();
    } 


    private void Start()
    {
        UpdataAllConnections();
    }

    private void UpdateSkillPointsUI()
    {
        skillPointsText.text = skillPoints.ToString();
    }
    public void UnlockDefaultSkills()
    {
        skillManager = FindAnyObjectByType<Player_SkillManager>();
        allTreeNodes = GetComponentsInChildren<UI_TreeNode>();

        foreach (var node in allTreeNodes)
            node.UnlockDefaultSkills();
    }

    [ContextMenu("Reset Skill Tree")]
    public void RefundAllSkills()
    {
        UI_TreeNode[] skillNodes = GetComponentsInChildren<UI_TreeNode>();

        foreach (var node in skillNodes)
            node.Refund();
    }

    [ContextMenu("UpdateAllConnection")]
    public void UpdataAllConnections()
    {
        foreach (var node in parentNodes)
        {
            node.UpdateAllConnections();
        }

    }

    public void LoadData(GameData gameData)
    {
        skillPoints = gameData.skillPoints;

        foreach(var node in allTreeNodes)
        {
            string skillName = node.skillData.skillName;

            if (gameData.skillTreeUI.TryGetValue(skillName, out bool unLocked) && unLocked)
                node.UnlockWithSaveData();
        }

        foreach(var skill in skillManager.allSkills)
        {
            if(gameData.skillUpgrades.TryGetValue(skill.GetSkillType(),out SkillUpgradeType skillUpgradeType))
            {
                var upgradeTreeNode = allTreeNodes.FirstOrDefault(node => node.skillData.upgradeData.skillUpgradeType == skillUpgradeType);

                if( upgradeTreeNode != null)
                    skill.SetSkillUpgrade(upgradeTreeNode.skillData);
            }

        }

        UpdateSkillPointsUI();
    }

    public void SaveData(ref GameData gameData)
    {
        gameData.skillPoints = skillPoints;
        gameData.skillTreeUI.Clear();
        gameData.skillUpgrades.Clear();

        foreach (var node in allTreeNodes)
        {
            string skillName = node.skillData.skillName;
            gameData.skillTreeUI[skillName] = node.isUnlocked;
        }

        foreach(var skill in skillManager.allSkills)
        {
            gameData.skillUpgrades[skill.GetSkillType()] = skill.GetSkillUpgradeType();
        }

    }
}
