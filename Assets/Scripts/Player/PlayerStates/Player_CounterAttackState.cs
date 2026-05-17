using UnityEngine;

public class Player_CounterAttackState : PlayerState
{
    private Player_Combat combat;
    private bool counterSomeone;

    // 【新增】配置弹反的有效判定时间（比如 0.2 秒）
    private float parryWindow = 0.2f;

    // 你可以在这里定义弹反音效在 AudioDataBase 中的名字
    private string parrySoundName = "PlayerParry";

    public Player_CounterAttackState(Player player, StateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        combat = player.GetComponent<Player_Combat>();
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = combat.GetCounterRecoveryDuration();
        counterSomeone = combat.CounterAttackPerformed();

        anim.SetBool("counterAttackPerformed", counterSomeone);
        // 【新增】：一进入弹反状态，立刻开始计算冷却！(防止玩家连续狂按)
        player.StartCounterCooldown();

        // 如果第一帧就弹反成功了，直接卡肉并播放音效
        if (counterSomeone)
        {
            // 停顿 0.15 秒，这个数值是“清脆手感”的黄金时间
            player.TriggerHitStop(0.15f);

            // 【新增奖励】：完美弹反立刻重置CD！
            player.ResetCounterCooldown();

            // 【新增】：播放弹反音效
            if (AudioManager.instance != null)
                AudioManager.instance.PlayGlobalSFX(parrySoundName);
        }
    }

    public override void Update()
    {
        base.Update();

        // 只有在地面上弹反时，才锁死 X 轴速度防止滑步。
        // 如果是在空中（下落跑酷），绝对不锁死 X 轴，以保留弹反获得的反推力！
        if (player.groundDetected)
        {
            player.SetVelocity(0, rb.linearVelocity.y);
        }

        // 如果第一帧没弹到，且还在"弹反有效时间"内，就持续检测！
        // stateTimer 初始值是 recovery duration，所以它大于 (总时长 - 判定时长) 时代表处于判定区间
        float timePassed = combat.GetCounterRecoveryDuration() - stateTimer;

        if (!counterSomeone && timePassed <= parryWindow)
        {
            counterSomeone = combat.CounterAttackPerformed();

            if (counterSomeone)
            {
                // 如果在持续期间内弹反成功了，立刻播放成功动画并触发时停等奖励
                anim.SetBool("counterAttackPerformed", true);

                // 触发 0.15 秒的超级卡肉！
                player.TriggerHitStop(0.15f);

                // 【新增核心防御】：弹反成功后，强行关闭玩家的受伤开关，赋予 0.5 秒无敌时间！
                // 这样就算敌人的武器还没收回去，也不会在弹反结束后立刻打伤你。
                player.health.SetCanTakeDamage(false);
                player.health.Invoke("ResetCanTakeDamage", 0.1f);

                // 【新增奖励】：在有效窗口内弹反成功，立刻重置CD！
                player.ResetCounterCooldown();

                // 【新增】：播放弹反音效
                if (AudioManager.instance != null)
                    AudioManager.instance.PlayGlobalSFX(parrySoundName);
            }
        }

        if (triggerCalled)
            stateMachine.ChangeState(player.idleState);

        if (stateTimer < 0 && counterSomeone == false)
            stateMachine.ChangeState(player.idleState);
    }

    // 【新增】：公开一个方法供 Health 系统查询，当前是否处于“有效弹反保护期”
    public bool IsInParryWindow()
    {
        // 计算当前处于该状态的时间
        float timePassed = combat.GetCounterRecoveryDuration() - stateTimer;

        // 如果已经弹反成功，或者还在判定窗口(parryWindow)内，都算作有效保护期
        return counterSomeone || timePassed <= parryWindow;
    }
}

