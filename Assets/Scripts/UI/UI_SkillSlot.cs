using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot : MonoBehaviour
{
    private UI ui;
    private Image skillIcon;
    private RectTransform rect;
    private Button button;

    private Skill_DataSO skillData;

    public SkillType skillType;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private string inputKeyName;
    [SerializeField] private TextMeshProUGUI inputKeyText;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        button = GetComponentInParent<Button>();
        skillIcon = GetComponent<Image>();
        rect = GetComponent<RectTransform>();

    }

    public void SetupSkillSlot(Skill_DataSO selectedSkill)
    {
        this.skillData = selectedSkill;

        Color color = Color.black; color.a = .6f;
        cooldownImage.color = color;

        inputKeyText.text = inputKeyName;
        skillIcon.sprite = selectedSkill.icon;
    }

    public void StartCoolDown(float cooldown)
    {
        cooldownImage.fillAmount = 1;
        StartCoroutine(CoolDownCo(cooldown));

    }

    public void ResetCoolDown() => cooldownImage.fillAmount = 0;

    private IEnumerator CoolDownCo(float duration)
    {
        float timePassed = 0;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            cooldownImage.fillAmount = 1f - (timePassed / duration);
            yield return null;
        }
        cooldownImage.fillAmount = 0;

    }

}
