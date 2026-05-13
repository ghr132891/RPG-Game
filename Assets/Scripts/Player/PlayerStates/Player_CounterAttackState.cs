using UnityEngine;

public class Player_CounterAttackState : PlayerState
{
    private Player_Combat combat;
    private bool counterSomeone;

    // 【新增】配置弹反的有效判定时间（比如 0.2 秒）
    private float parryWindow = 0.3f;

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

        // 如果第一帧就弹反成功了，直接卡肉并播放音效
        if (counterSomeone)
        {
            // 停顿 0.15 秒，这个数值是“清脆手感”的黄金时间
            player.TriggerHitStop(0.15f);

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
}

