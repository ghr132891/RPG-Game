using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Skill Data", fileName = "Skill Data - ")]
public class Skill_DataSO : ScriptableObject
{

    [Header("Skill Description")]
    public string skillName;
    [TextArea]
    public string skillDescription;
    public Sprite icon;

    [Header("Unlock & Update")]
    public int cost;
    public SkillType skillType;
    public bool unlockedBydefault;
    public UpgradeData upgradeData;

}
 
[System.Serializable]
public class UpgradeData
{
    public SkillUpgradeType skillUpgradeType;
    public float cooldown;
    public DamageScaleData damageScaleData;


}

