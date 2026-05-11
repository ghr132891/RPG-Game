using UnityEngine;


public class Skill_Base : MonoBehaviour
{
    public Player player { get; private set; }
    public Player_SkillManager skillManager { get; private set; }
    public DamageScaleData damageScaleData { get; private set; }

    [Header("General details")]
    [SerializeField] protected SkillType skillType;
    [SerializeField] protected SkillUpgradeType skillUpgradeType;
    [SerializeField] protected float cooldown;
    private float lastTimeUsed;
    protected Transform lastTarget;


    protected virtual void Awake()
    {
        skillManager = GetComponentInParent<Player_SkillManager>();
        player = GetComponentInParent<Player>();

        lastTimeUsed -= cooldown;
        damageScaleData = new DamageScaleData();
    }

    public virtual void TryUseSkill()
    {

    }

    public void SetSkillUpgrade(Skill_DataSO skillData)
    {
        UpgradeData upgrade = skillData.upgradeData;

        skillUpgradeType = upgrade.skillUpgradeType;
        cooldown = upgrade.cooldown;
        damageScaleData = upgrade.damageScaleData;

        var uiSlot = player.ui.inGameUI.GetSkillSlot(skillType);
        if (uiSlot != null)
        {
            uiSlot.SetupSkillSlot(skillData);
        }
        ResetCooldown();

    }
    public virtual bool CanUseSkill()
    {
        if (skillUpgradeType == SkillUpgradeType.None)
            return false;

        if (OnCooldown())
        {
            Debug.Log("On Cooldown.");
            return false;
        }

        return true;

    }

    protected bool Unlocked(SkillUpgradeType upgradeToCheck) => skillUpgradeType == upgradeToCheck;
    public SkillUpgradeType GetSkillUpgradeType() => skillUpgradeType;
    public SkillType GetSkillType() => skillType;
    public bool OnCooldown() => Time.time < lastTimeUsed + cooldown;
    public void SetSkillOnCooldown()
    {
        var uiSlot = player.ui.inGameUI.GetSkillSlot(skillType);
        if (uiSlot != null)
        {
            uiSlot.StartCoolDown(cooldown);
        }
        lastTimeUsed = Time.time;
    }


    public void ReduceCooldownBy(float cooldownReduction) => lastTimeUsed += cooldownReduction;

    public void ResetCooldown()
    {
        var uiSlot = player.ui.inGameUI.GetSkillSlot(skillType);
        if (uiSlot != null)
        {
            uiSlot.ResetCoolDown();
        }

        lastTimeUsed = Time.time - cooldown;
    }


}
