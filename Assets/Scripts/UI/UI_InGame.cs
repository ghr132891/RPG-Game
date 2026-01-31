using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGame : MonoBehaviour
{
    private Player player;
    private UI_SkillSlot[] skillSlots;


    [SerializeField] private RectTransform healthRect;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    private float maxWidth;

    private void Awake()
    {
        maxWidth = healthRect.sizeDelta.x;

        skillSlots = GetComponentsInChildren<UI_SkillSlot>(true);
    }
    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        player.health.OnHealthUpdate += UpdateHealthBar;
    }

    public UI_SkillSlot GetSkillSlot(SkillType skillType)
    {
        foreach (var slot in skillSlots)
        {
            if(slot.skillType == skillType)
                return slot;
        }

        return null;
    }

    private void UpdateHealthBar()
    {
        float currentHealth = Mathf.RoundToInt(player.health.GetCurrentHealth());
        float maxHelath = player.stats.GetMaxHealth();
        float baseHealthLength = 200;
        float sizePercent = currentHealth / baseHealthLength;
        float targetWidth = maxWidth * sizePercent;
        float sizeDifference = Mathf.Abs(maxHelath  - healthRect.sizeDelta.x);

        if (sizeDifference > .1f)
            healthRect.sizeDelta = new Vector2(maxHelath,healthRect.sizeDelta.y);

        healthText.text = currentHealth +"/" + maxHelath;
        healthSlider.value  =player.health.GetHealthPercent();
    }

}
