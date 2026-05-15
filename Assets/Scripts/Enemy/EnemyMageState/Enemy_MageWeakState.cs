using UnityEngine;

public class Enemy_MageWeakState : EnemyState
{
    private Enemy_Mage enemyMage;
    private float initialHealth;
    private bool hasTriggeredRewind;

    public Enemy_MageWeakState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as Enemy_Mage;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemyMage.weakDuration;
        initialHealth = enemyMage.health.GetCurrentHealth();
        hasTriggeredRewind = false;

        enemyMage.vfx.DoImageEchoEffect(1f);
        enemyMage.EvaluateVulnerability();
    }

    public override void Update()
    {
        base.Update();

        if (enemyMage.health.isDead) return;

        // 受到伤害打断
        if (!hasTriggeredRewind && enemyMage.health.GetCurrentHealth() < initialHealth)
        {
            hasTriggeredRewind = true;
            TriggerTimeRewind();
            stateMachine.ChangeState(enemyMage.mageBattleState);
            return;
        }

        // 时间结束自然醒来
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemyMage.mageBattleState);
        }
    }

    public override void Exit()
    {
        // ==========================================
        // 【终极视觉防卡死】：临时关闭 Animator 的恢复动画分支
        // 防止状态机已经切到了战斗，但动画器却拐进 "虚弱回复" 动画里卡死出不来！
        // ==========================================
        bool originalRecovery = enemyMage.anim.GetBool("hasStunRecovery");
        enemyMage.anim.SetBool("hasStunRecovery", false);

        base.Exit(); // 内部会将 "stunned" 设为 false。此时 Animator 会直接跳过回复动画，秒切回基础动作

        // 恢复参数，以免影响后续正常的眩晕逻辑
        enemyMage.anim.SetBool("hasStunRecovery", originalRecovery);

        enemyMage.vfx.StopImageEchoEffect();

        if (enemyMage.health.isDead)
        {
            return;
        }

        // 兜底检测
        if (!hasTriggeredRewind && enemyMage.health.GetCurrentHealth() < initialHealth)
        {
            hasTriggeredRewind = true;
            TriggerTimeRewind();
        }

        enemyMage.EvaluateVulnerability();
    }

    private void TriggerTimeRewind()
    {
        Debug.Log("<color=red>法师受到攻击，触发时间回退！玩家回到 5 秒前！</color>");

        Player_TimeRewinder playerRewinder = UnityEngine.Object.FindAnyObjectByType<Player_TimeRewinder>();

        if (playerRewinder != null)
        {
            playerRewinder.StartRewind();
            enemyMage.FreezeDuringTimeRewind(playerRewinder);
        }
        else
        {
            Debug.LogError("【严重错误】全图搜索失败！场景中找不到 Player_TimeRewinder！");
        }
    }
}