    using UnityEngine;

public class Player_Health : Entity_Health
{
    private Player player;
    [Header("SFX (玩家受击音效)")]
    [SerializeField] private string hurtSoundName = "PlayerHurt";
    [SerializeField] private string deathSoundName = "PlayerDeath"; // 新增死亡音效名称

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
            Die();
    }

    public override bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        // ========================================================
        // 【第一层绝对防御】：只要处于弹反动作中，无视一切物理和魔法伤害！
        // 这里的 return false 会直接掐断后续逻辑，绝对不会播放受击音效。
        // ========================================================
        if (player.stateMachine.currentState == player.counterAttackState)
        {
            Debug.Log("【完美格挡】处于弹反状态中，拦截伤害！");
            return false;
        }

        // 调用父类扣血逻辑（护甲减伤、无敌帧判断等）
        bool tookDamage = base.TakeDamage(damage, elementalDamage, element, damageDealer);

        // 如果真的扣血了（破防了 / 没闪避掉 / 没弹反成功）
        if (tookDamage)
        {
            // 1. 播放受伤音效
            if (!string.IsNullOrEmpty(hurtSoundName))
            {
                AudioManager.instance.PlayGlobalSFX(hurtSoundName);
            }

            // 2. 【核心修复】：剥夺攻击者的被弹反资格！
            // 既然伤害已经造成，说明玩家按晚了，立刻强行关闭敌人的弹反窗口！
            if (damageDealer != null)
            {
                Enemy enemy = damageDealer.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.EnableCounterWindow(false);
                    // 现在，就算玩家在受伤后马上狂按弹反键，也绝对弹不到这个敌人了！
                }
            }
        }

        return tookDamage;
    }
    protected override void Die()
    {

        if (!isDead && !string.IsNullOrEmpty(deathSoundName))
        {
            AudioManager.instance.PlayGlobalSFX(deathSoundName); // 播放死亡音效
        }
        base.Die();

        player.ui.OpenDeathScreenUI();
        //GameManager.instance.SetLastPlayerPosition(transform.position);
        //GameManager.instance.RestartScene();

    }

}
