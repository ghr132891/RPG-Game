using System.Collections;
using UnityEngine;

public class Enemy_Mage : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_MageRetreatState mageRetreatState { get; private set; }
    public Enemy_MageBattleState mageBattleState { get; private set; }
    public Enemy_MageSpellCastState mageSpellCastState { get; private set; }
    public Enemy_MageWeakState mageWeakState { get; private set; }

    [Header("Mage Specifics")]
    [SerializeField] private GameObject spellPrefab;
    [SerializeField] private Transform spellStartPosition;
    public int amountToCast = 3;
    [SerializeField] private float spellCastCooldown = .3f;

    [Header("Counter To Spell Settings")]
    public int requiredParriesToCast = 3; // 触发扔炸弹所需的弹反次数
    public int currentParryCount { get; private set; } // 当前已弹反的次数
    public bool spellCastPerformed { get; private set; }

    [Header("Bomb Parry Settings")]
    public float weakDuration = 5f; // 虚弱状态持续时间
    public float allParriedBonusDamage = 20f; // 全部弹反时，法师受到的额外惩罚伤害
    public int bombsParriedCount { get; private set; } // 成功弹反的炸弹计数器

    [Header("Spell Rhythm Settings")]
    [Tooltip("控制每次扔炸弹后的停顿时间。例如填入 0.3, 0.3, 1.0 就是 哒-哒--哒 的节奏")]
    public float[] castRhythm;

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
        deadState = new Enemy_DeadState(this, stateMachine, "idle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

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
    }

    //当炸弹被弹反时调用
    public void OnBombParried()
    {
        bombsParriedCount++;
    }
    public override void SpecialAttack()
    {
        StartCoroutine(CastSpellCo());
    }
    public void SetSpellCastPerformed(bool performed) => spellCastPerformed = performed;


    private IEnumerator CastSpellCo()
    {
        bombsParriedCount = 0;

        for (int i = 0; i < amountToCast; i++)
        {
            Transform target = player != null ? player : Player.instance.transform;

            Enemy_MageProjectile projectile
                = Instantiate(spellPrefab, spellStartPosition.position, Quaternion.identity).GetComponent<Enemy_MageProjectile>();

            projectile.SetupProjectile(target, combat);

            // 【核心修改：动态节奏提取】
            float currentWaitTime = 1f; // 默认保底 1 秒

            // 检查你是否在 Unity 面板里配置了节奏数组
            if (castRhythm != null && castRhythm.Length > 0)
            {
                // 使用取余符号 (%) 是为了防止越界报错！
                // 比如你设置了扔 5 个炸弹，但节奏数组只填了 3 个数字，它会循环读取 (0,1,2,0,1)
                currentWaitTime = castRhythm[i % castRhythm.Length];
            }
            else
            {
                // 如果你忘了配置节奏数组，就用你之前的默认冷却时间兜底
                currentWaitTime = spellCastCooldown;
            }

            yield return new WaitForSeconds(currentWaitTime);
        }

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

    public override void OnStunFinished()
    {
        // 如果计数器满了
        if (currentParryCount >= requiredParriesToCast)
        {
            currentParryCount = 0; // 重置计数器

            // 拦截原本去 Idle 的路线，强行转入撤退扔炸弹状态！
            stateMachine.ChangeState(mageRetreatState);
        }
        else
        {
            // 如果次数没满，就按普通怪物那样，乖乖进入 Idle 继续近战
            base.OnStunFinished();
        }
    }

    public bool CanNotMoveBackwards()
    {
        bool detectedWall = Physics2D.Raycast(behindCollsionCheck.position, Vector2.right * -facingDir, 4f, whatIsGround);
        bool noGround = Physics2D.Raycast(behindCollsionCheck.position, Vector2.down, 4f, whatIsGround) == false;

        return noGround || detectedWall;
    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.DrawLine(behindCollsionCheck.position,
            new Vector3(behindCollsionCheck.position.x + (4f * -facingDir), behindCollsionCheck.position.y));

        Gizmos.DrawLine(behindCollsionCheck.position,
            new Vector3(behindCollsionCheck.position.x, behindCollsionCheck.position.y - 4f));
    }
}
