using System.Collections;
using UnityEngine;

public class Enemy_Mage : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_MageRetreatState mageRetreatState { get; private set; }
    public Enemy_MageBattleState mageBattleState { get; private set; }
    public Enemy_MageSpellCastState mageSpellCastState { get; private set; }
    public Enemy_MageWeakState mageWeakState { get; private set; }

    public Enemy_MageDashAttackState mageDashAttackState { get; private set; }

    [Header("Mage Specifics")]
    [SerializeField] private GameObject spellPrefab;
    // 【新增】：用于存放单次发射炸弹时的独立特效（如魔法阵闪烁、能量爆裂）
    [SerializeField] private GameObject spellCastVFXPrefab;
    [SerializeField] private Transform spellStartPosition;
    public int amountToCast = 3;
    [SerializeField] private float spellCastCooldown = .3f;

    [Header("Counter To Spell Settings")]
    public int requiredParriesToCast = 3; // 触发扔炸弹所需的弹反次数
    public int currentParryCount { get; private set; } // 当前已弹反的次数
    public bool spellCastPerformed { get; private set; }

    [Header("Bomb Parry Settings")]
    [Tooltip("至少需要弹反多少个炸弹，法师施法结束后才会进入普通虚弱状态")]
    public int requiredParriesToWeak = 1;
    [Tooltip("需要弹反多少个炸弹，才能触发完美弹反（造成额外真实伤害），通常等于 amountToCast")]
    public int requiredParriesToPerfect = 3;
    public float weakDuration = 5f; // 虚弱状态持续时间
    public float allParriedBonusDamage = 1f; // 全部弹反时，法师受到的额外惩罚伤害
    public int parriedBombsReturnedCount { get; private set; } // 已经飞回到法师身上的炸弹计数器
    public int bombsParriedCount { get; private set; } // 成功弹反的炸弹计数器

    [Header("Spell Rhythm Settings")]
    [Tooltip("控制每次扔炸弹后的停顿时间。例如填入 0.3, 0.3, 1.0 就是 哒-哒--哒 的节奏")]
    public float[] castRhythm;

    [Header("Dash Attack Settings")]
    public float dashAttackProbability = 0.3f; // 触发冲刺大斩击的概率 (0.3 = 30%)
    public float backDashSpeed = 10f;          // 后退时的速度
    public float backDashDuration = 0.5f;      // 后退持续的时间
    public float forwardDashSpeed = 15f;       // 向前冲刺斩击的速度
    public bool isDoingDashAttack;             // 标记当前是否正在执行此技能

    [SerializeField] private Transform behindCollsionCheck;
    [SerializeField] private bool hasStunRecoveryAnimation = true;

    [Space]
    public float retreatCoolDown = 5;
    public float retreatMaxDistance = 8;
    public float retreatSpeed = 15;


    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        deadState = new Enemy_DeadState(this, stateMachine, "dead");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");
        mageDashAttackState = new Enemy_MageDashAttackState(this, stateMachine, "dashAttack", this);

        mageSpellCastState = new Enemy_MageSpellCastState(this, stateMachine, "spellCast");
        mageRetreatState = new Enemy_MageRetreatState(this, stateMachine, "battle");
        mageBattleState = new Enemy_MageBattleState(this, stateMachine, "battle");
        mageWeakState = new Enemy_MageWeakState(this, stateMachine, "stunned");
        battleState = mageBattleState;

        anim.SetBool("hasStunRecovery", hasStunRecoveryAnimation);
    }

    protected override void Start()
    {
        base.Start();

        stateMachine.Initialize(idleState);

        // 【新增】：在 Start 中订阅世界切换事件
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnWorldChanged += CheckMirrorWorld;

            // 手动调用一次，确保法师刚生成时的无敌状态是正确的
            CheckMirrorWorld(WorldManager.Instance.currentWorld);
        }
    }

    private void CheckMirrorWorld(WorldType worldType)
    {
        // 替换掉原来的逻辑，直接调用统一判定
        EvaluateVulnerability();
    }

    // 【新增】统一管理法师的受击状态与透明度表现
    public void EvaluateVulnerability()
    {
        bool inMirror = WorldManager.Instance != null && WorldManager.Instance.currentWorld == WorldType.Mirror;
        bool isWeak = stateMachine.currentState == mageWeakState;

        // 核心规则：只有在【镜像世界】且【处于虚弱状态】时，才可以受击
        if (inMirror && isWeak)
        {
            health.SetCanTakeDamage(true);
            GetComponentInChildren<SpriteRenderer>().color = Color.white;
        }
        else
        {
            health.SetCanTakeDamage(false);
            GetComponentInChildren<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy(); // 调用父类的销毁逻辑（如果父类 Enemy 也有 OnDestroy 的话）

        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.OnWorldChanged -= CheckMirrorWorld;
        }
    }

    protected override void HandlePlayerDeath()
    {
        // 1. 强制停止法师身上所有正在运行的协程（最核心：这会立刻中断 CastSpellCo 扔炸弹的循环）
        StopAllCoroutines();

        // 2. 重置法师特有的状态标志位，防止下次遇到玩家时状态错乱
        spellCastPerformed = false;
        isDoingDashAttack = false;

        // 3. 调用父类的逻辑，将状态机安全地切换回 idleState
        base.HandlePlayerDeath();
    }

    public override void OnStunFinished()
    {
        // 【核心修改】：检测是否是在“冲刺斩击”过程中被弹反打断的
        if (isDoingDashAttack)
        {
            isDoingDashAttack = false; // 重置标记
            currentParryCount = 0;     // (可选) 重置普通弹反次数计数器

            // 直接强行转入撤退扔炸弹状态！
            stateMachine.ChangeState(mageRetreatState);
            return;
        }

        // 原本的逻辑：如果计数器满了
        if (currentParryCount >= requiredParriesToCast)
        {
            currentParryCount = 0; // 重置计数器
            stateMachine.ChangeState(mageRetreatState);
        }
        else
        {
            // 如果次数没满，继续近战
            base.OnStunFinished();
        }
    }

    public void ResetDashAttackFlag()
    {
        isDoingDashAttack = false;
    }

    // 给状态机调用的公共方法
    public void FreezeDuringTimeRewind(Player_TimeRewinder playerRewinder)
    {
        StartCoroutine(FreezeMageRoutine(playerRewinder));
    }

    private IEnumerator FreezeMageRoutine(Player_TimeRewinder playerRewinder)
    {
        // 1. 记录被冻结前的状态
        RigidbodyType2D originalBodyType = rb.bodyType;
        Vector2 originalVelocity = rb.linearVelocity;
        float originalAnimSpeed = anim.speed;

        // 2. 暂停法师的行动
        // 禁用自身脚本，这会停止 Entity.cs 中的 Update()，从而彻底暂停 stateMachine 的运作
        this.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        anim.speed = 0;

        // 3. 挂起协程，等待玩家回溯结束
        yield return new WaitWhile(() => playerRewinder.isRewinding);

        // 4. 回溯结束，恢复法师的行动
        anim.speed = originalAnimSpeed > 0 ? originalAnimSpeed : 1;
        rb.bodyType = originalBodyType;
        rb.linearVelocity = originalVelocity;
        this.enabled = true; // 重新启用脚本，状态机从暂停的那一帧完美继续
    }



    //当炸弹被弹反时调用
    public void OnBombParried()
    {
        bombsParriedCount++;

        if (bombsParriedCount >= requiredParriesToWeak)
        {
            StartCoroutine(ParryFlashRoutine());
        }
    }

    // 【新增】：当被弹反的炸弹飞回到法师身上时调用
    public void OnParriedBombHit()
    {
        parriedBombsReturnedCount++;

        // 如果飞回来的炸弹数量，等于我们成功弹反的数量，说明这是最后一颗！
        // 并且如果总弹反数满足了完美弹反的要求，就在此刻结算真实伤害！
        if (parriedBombsReturnedCount == bombsParriedCount && bombsParriedCount >= requiredParriesToPerfect)
        {
            // 临时强制赋予受击权限
            health.SetCanTakeDamage(true);

            IDamagable damagable = GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(allParriedBonusDamage, 0, ElementType.None, transform);
            }
        }
    }

    private IEnumerator ParryFlashRoutine()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red; // 闪红光
            yield return new WaitForSeconds(0.15f);
            EvaluateVulnerability(); // 闪烁结束后，恢复原本该有的颜色（半透明或正常）
        }
    }
    public override void SpecialAttack()
    {
        StartCoroutine(CastSpellCo());
    }
    public void SetSpellCastPerformed(bool performed) => spellCastPerformed = performed;


    private IEnumerator CastSpellCo()
    {
        bombsParriedCount = 0;
        parriedBombsReturnedCount = 0; // 【新增】每次扔炸弹前清零

        for (int i = 0; i < amountToCast; i++)
        {
            // 1. 【核心修复】：先读取节奏时间
            float waitTime = 1f;
            if (castRhythm != null && castRhythm.Length > 0)
            {
                waitTime = castRhythm[i % castRhythm.Length];
            }
            else
            {
                waitTime = spellCastCooldown;
            }

            // 2. 先等待！这相当于法师的“施法前摇”或者两发之间的“攻击间隔”
            yield return new WaitForSeconds(waitTime);

            if (spellCastVFXPrefab != null)
            {
                // 在法杖顶端（或发球点）生成特效
                GameObject vfx = Instantiate(spellCastVFXPrefab, transform.position, Quaternion.identity);
                // 如果法师朝左（facingDir 为 -1），就让特效也跟着水平翻转
                if (facingDir == -1)
                {
                    vfx.transform.Rotate(0, 180, 0);
                }

                // 为了防止内存泄漏，确保特效能在播放完毕后自动销毁
                // （假设特效持续 1 秒，如果你的特效本身自带销毁脚本，可以删除这行）
                Destroy(vfx, 1f);
            }

            // 3. 时间到了，再扔出对应的炸弹！
            Transform target = player != null ? player : Player.instance.transform;
            Enemy_MageProjectile projectile = Instantiate(spellPrefab, spellStartPosition.position, Quaternion.identity).GetComponent<Enemy_MageProjectile>();
            projectile.SetupProjectile(target, combat);
        }

        // 最后一发扔完后，立刻通知状态机，进入那 1.5 秒的弹反等待期
        SetSpellCastPerformed(true);
    }
    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        // 每次被弹反都增加计数器
        currentParryCount++;

        // 无论次数够不够，先强制进入硬直状态（挨打就要立正）
        stateMachine.ChangeState(stunnedState);
    }



    public bool CanNotMoveBackwards()
    {
        bool detectedWall = Physics2D.Raycast(behindCollsionCheck.position, Vector2.right * -facingDir, 4f, whatIsGround);
        bool noGround = Physics2D.Raycast(behindCollsionCheck.position, Vector2.down, 6f, whatIsGround) == false;

        return noGround || detectedWall;
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        StopAllCoroutines();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        GetComponentInChildren<SpriteRenderer>().color = Color.white;

        // 【最核心的修复】：手动强制状态机切换到死亡状态！
        // 只有进入了这个状态，代码才会执行 anim.SetBool("dead", true) 去触发动画
        if (deadState != null)
        {
            stateMachine.ChangeState(deadState);
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawLine(behindCollsionCheck.position,
            new Vector3(behindCollsionCheck.position.x + (4f * -facingDir), behindCollsionCheck.position.y));

        Gizmos.DrawLine(behindCollsionCheck.position,
            new Vector3(behindCollsionCheck.position.x, behindCollsionCheck.position.y - 6f));
    }



}
