using UnityEngine;

public class UI_SkillTree : MonoBehaviour
{
    [SerializeField] private int skillPoints;
    [SerializeField] private UI_TreeConnectHandler[] parentNodes;
    public Player_SkillManager skillManager { get; private set; }

    public bool EnoughSkillPoints(int cost) => skillPoints >= cost;
    public void RemoveSkillPoints(int cost) => skillPoints -= cost;

    public void AddSkillPoints(int cost) => skillPoints += cost;

    private void Awake()
    {
        skillManager = FindAnyObjectByType<Player_SkillManager>(); 
    }
    private void Start()
    {
        UpdataAllConnections();
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
}
