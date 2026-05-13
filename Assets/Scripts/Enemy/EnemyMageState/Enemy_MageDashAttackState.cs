using UnityEngine;

public class Enemy_MageDashAttackState : EnemyState
{
    private Enemy_Mage mage;
    private int attackStep; // 0: 正在后退, 1: 瞬移冲刺中, 2: 原地挥刀斩击中
    private float dashAttackTimer;
    private int dashDirection;

    // 用来记录真正的攻击动画参数名
    private string dashAttackAnimName;

    // 【重要修改】：我们把传给父类的参数写死成 "move"
    // 这样法师在后撤和瞬移时，只会播放移动姿势，而不会过早挥刀！
    public Enemy_MageDashAttackState(Enemy _enemyBase, StateMachine _stateMachine, string _animBoolName, Enemy_Mage _mage)
        : base(_enemyBase, _stateMachine, "move")
    {
        this.mage = _mage;
        // 存下传进来的 "dashAttack" 字符串，等贴脸了我们再手动播放
        this.dashAttackAnimName = _animBoolName;
    }

    public override void Enter()
    {
        base.Enter(); // 这句会自动让法师的 "move" 动画变为 true
        mage.isDoingDashAttack = true;
        attackStep = 0;
        dashAttackTimer = mage.backDashDuration;

        // 阶段 0：开始后撤
        mage.rb.linearVelocity = new Vector2(-mage.facingDir * mage.backDashSpeed, mage.rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (attackStep == 0) // 正在后退
        {
            mage.rb.linearVelocity = new Vector2(-mage.facingDir * mage.backDashSpeed, mage.rb.linearVelocity.y);
            dashAttackTimer -= Time.deltaTime;

            // 后退如果碰到墙壁，提前结束倒退
            if (mage.CanNotMoveBackwards())
            {
                dashAttackTimer = 0;
            }

            if (dashAttackTimer <= 0)
            {
                // ================= 进入阶段 1：锁定玩家并开始瞬移 =================
                attackStep = 1;
                mage.EnableCounterWindow(true); // 开启弹反窗口

                Transform target = mage.player != null ? mage.player : Player.instance.transform;

                if (target != null)
                {
                    dashDirection = target.position.x > mage.transform.position.x ? 1 : -1;
                    if (mage.facingDir != dashDirection)
                    {
                        mage.Flip(); // 强制转身死死盯住玩家
                    }
                }
                else
                {
                    dashDirection = mage.facingDir;
                }

                // 给予极高的瞬移速度
                mage.SetVelocity(dashDirection * mage.forwardDashSpeed, rb.linearVelocity.y);
            }
        }
        else if (attackStep == 1) // 瞬移冲刺中
        {
            Transform target = mage.player != null ? mage.player : Player.instance.transform;

            // 检查是否贴脸了（距离小于 1.5）或者是否撞墙了（防止被墙卡住无限跑）
            bool reachedPlayer = target != null && Mathf.Abs(mage.transform.position.x - target.position.x) < 1.5f;
            bool hitWall = mage.wallDetected;

            if (reachedPlayer || hitWall)
            {
                // ================= 进入阶段 2：贴脸刹车，挥出大斩击！ =================
                mage.SetVelocity(0, rb.linearVelocity.y); // 立刻刹车停在脸上

                attackStep = 2; // 进入斩击阶段

                // 关掉移动动画，正式播放大斩击动画！
                mage.anim.SetBool("move", false);
                mage.anim.SetBool(dashAttackAnimName, true);
            }
            else
            {
                // 如果还没贴脸，继续保持超高速冲刺
                mage.SetVelocity(dashDirection * mage.forwardDashSpeed, rb.linearVelocity.y);
            }
        }
        else if (attackStep == 2) // 原地挥刀斩击中
        {
            // 在挥刀期间，死死钉在原地，绝对不能滑动
            mage.SetVelocity(0, rb.linearVelocity.y);

            // 等待真正的斩击动画播放完毕 (依赖你在动画最后一帧挂载的 AnimationFinishTrigger 事件)
            if (triggerCalled)
            {
                stateMachine.ChangeState(mage.idleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit(); // 自动把 "move" 设为 false

        // 离开状态时，确保把大斩击动画也关掉
        mage.anim.SetBool(dashAttackAnimName, false);
        mage.EnableCounterWindow(false);

        if (triggerCalled)
        {
            mage.ResetDashAttackFlag();
        }
    }
}