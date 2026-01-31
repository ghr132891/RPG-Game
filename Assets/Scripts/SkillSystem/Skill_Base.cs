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
        UpgradeData upgrade =skillData.upgradeData;

        skillUpgradeType = upgrade.skillUpgradeType;
        cooldown = upgrade.cooldown;
        damageScaleData = upgrade.damageScaleData;

        player.ui.inGameUI.GetSkillSlot(skillType).SetupSkillSlot(skillData);
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

    public bool OnCooldown() => Time.time < lastTimeUsed + cooldown;
    public void SetSkillOnCooldown() 
    {
        player.ui.inGameUI.GetSkillSlot(skillType).StartCoolDown(cooldown);
        lastTimeUsed = Time.time;
    } 


    public void ReduceCooldownBy(float cooldownReduction) => lastTimeUsed += cooldownReduction;

    public void ResetCooldown() 
    {
        player.ui.inGameUI.GetSkillSlot(skillType).ResetCoolDown();

        lastTimeUsed = Time.time - cooldown;
    }


}
