using UnityEngine;

public class UI : MonoBehaviour
{
    public  UI_SkillToolTip skillToolTip;
    private UI_SkillTree skillTree;
    private bool skillTreeEnabled;

    private void Awake()
    {
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        skillTree = GetComponentInChildren<UI_SkillTree>(true);
    }

    public void ToggleSkillTreeUI()
    {
        skillTreeEnabled = !skillTreeEnabled;
        skillTree.gameObject.SetActive(skillTreeEnabled);
        skillToolTip.ShowToolTip(false, null);

    }

}
