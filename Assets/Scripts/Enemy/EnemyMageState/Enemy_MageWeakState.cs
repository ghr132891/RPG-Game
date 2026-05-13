using UnityEngine;

public class Enemy_MageWeakState : EnemyState
{
    private Enemy_Mage enemyMage;
    private float initialHealth;
    private bool hasTriggeredRewind; // 【新增】防止重复触发的锁

    public Enemy_MageWeakState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as Enemy_Mage;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = enemyMage.weakDuration;
        initialHealth = enemyMage.health.GetCurrentHealth();
        hasTriggeredRewind = false; // 重置锁

        enemyMage.vfx.DoImageEchoEffect(1f);
    }

    public override void Update()
    {
        base.Update();

        // 常规检测：如果某些持续伤害没有打断状态，这里依然能正常捕捉
        if (!hasTriggeredRewind && enemyMage.health.GetCurrentHealth() < initialHealth)
        {
            hasTriggeredRewind = true;
            TriggerTimeRewind();
            stateMachine.ChangeState(enemyMage.mageBattleState);
            return;
        }

        // 时间结束自然退出
        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemyMage.mageBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        enemyMage.vfx.StopImageEchoEffect();

        // 【核心机制：退出兜底拦截】
        // 如果状态是被底层的受击逻辑“强行打断”的，这里依然能抓到血量下降的铁证！
        if (!hasTriggeredRewind && enemyMage.health.GetCurrentHealth() < initialHealth)
        {
            hasTriggeredRewind = true;
            TriggerTimeRewind();
        }
    }

    private void TriggerTimeRewind()
    {
        Debug.Log("<color=red>法师受到攻击，触发时间回退！玩家回到 5 秒前！</color>");

        // 全图搜索，绝对能启动玩家的回溯组件！
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