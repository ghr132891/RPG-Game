using UnityEngine;

public class Enemy_Slime : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_SlimeDeadState slimeDeadState { get; set; }
    private Transform targetToChase;
    [SerializeField] private GameObject platformCollider;

    [Header("Slime Specifics")]
    [SerializeField] private GameObject childSlimePrefab;
    [SerializeField] private int amountOfSlimeToCreate = 2;

    [SerializeField] private bool hasStunRecoveryAnimation = true;


    // 【新增区域】：尖刺卡住机制
    public bool isStuckOnSpike { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        battleState = new Enemy_BattleState(this, stateMachine, "battle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

        slimeDeadState = new Enemy_SlimeDeadState(this, stateMachine, "idle");

        anim.SetBool("hasStunRecovery", hasStunRecoveryAnimation);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
        SetPlatformCollider(true);

        WorldManager.Instance.OnWorldChanged += HandleWorldChanged;

        // 【新增修复】：防止左脚踩右脚！强制忽略史莱姆主身体和跳板之间的物理碰撞！
        Collider2D mainCol = GetComponent<Collider2D>();
        Collider2D platformCol = platformCollider.GetComponent<Collider2D>();
        if (mainCol != null && platformCol != null)
        {
            Physics2D.IgnoreCollision(mainCol, platformCol, true);
        }
    }

    protected override void Update()
    {
        base.Update();

        // 【最核心逻辑】：每一帧都在监视！
        // 如果它正卡在尖刺上，且试图因为各种原因（比如玩家靠近）切入移动或战斗状态
        // 我们强行把它按回 idleState，这样它就会像个石头一样一动不动！
        if (isStuckOnSpike && stateMachine.currentState != slimeDeadState && stateMachine.currentState != idleState)
        {
            stateMachine.ChangeState(idleState);
        }
    }

    public void GetStuckOnSpike()
    {
        isStuckOnSpike = true;

        // 1. 强行将 X 轴速度清零防止滑行，保留 Y 轴让它能自然落地
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // 2. 只要它还没死，就强行让它切回 Idle 状态
        if (stateMachine.currentState != slimeDeadState)
        {
            stateMachine.ChangeState(idleState);
        }
    }

    public void UnstuckFromSpike()
    {
        isStuckOnSpike = false;
    }

    private void HandleWorldChanged(WorldType newWorld)
    {
        if (newWorld == WorldType.Mirror)
        {
            // 陷入虚弱：关闭物理碰撞（玩家可穿透），改变颜色或播放虚弱动画
            // 镜像界：关闭跳板，陷入虚弱
            SetPlatformCollider(false);
            GetComponent<Collider2D>().isTrigger = true;
            anim.SetBool("isWeak", true);
        }
        else
        {
            // 现实界：开启跳板，变硬
            SetPlatformCollider(true);
            // 恢复正常
            GetComponent<Collider2D>().isTrigger = false;
            anim.SetBool("isWeak", false);
        }
    }

    private void SetPlatformCollider(bool value)
    {
        if(platformCollider != null)
            platformCollider.SetActive(value);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // 记得注销事件，防止内存泄漏
        if (WorldManager.Instance != null)
            WorldManager.Instance.OnWorldChanged -= HandleWorldChanged;
    }

    public override void EntityDeath()
    {
        stateMachine.ChangeState(slimeDeadState);
    }
    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }

    public void CreateSlimeOnDeath()
    {
        if (childSlimePrefab == null)
            return;

        for (int i = 0; i < amountOfSlimeToCreate; i++)
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(0f, 0.5f), 0);
            GameObject newSlime = Instantiate(childSlimePrefab, transform.position + spawnOffset, Quaternion.identity);
            Enemy_Slime slimeScript = newSlime.GetComponent<Enemy_Slime>();

            slimeScript.stats.AdjustStatSetup(stats.resources, stats.offense, stats.defense, .6f, 1.2f);
            slimeScript.SetupSlimeAsChild(player);
        }
    }
    public void ApplyRespawnVelocity()
    {
        Vector2 velocity = new Vector2(stunnedVelocity.x * Random.Range(-1f, 1f), stunnedVelocity.y * Random.Range(1f, 2f));
        SetVelocity(velocity.x, velocity.y);
    }

    public void StartBattleStateCheck(Transform player)
    {
        TryEnterBattleState(player);
        InvokeRepeating(nameof(ReEnterBattleState), 0, .3f);
    }
    private void DelayedEnterBattle()
    {
        if (targetToChase != null)
        {
            StartBattleStateCheck(targetToChase);
        }
    }
    public void SetupSlimeAsChild(Transform targetPlayer)
    {
        targetToChase = targetPlayer;

        // 强制让小史莱姆一出生就进入 Stunned（眩晕/被击飞）状态
        // 这样能够防止自带的 Idle 或 Battle 状态把这股冲击力抹除
        stateMachine.Initialize(stunnedState);

        // 赋予炸开的随机物理速度
        ApplyRespawnVelocity();

        // 延迟 1 秒（等待被击飞、落地的物理表现结束）后，再让它去追击玩家
        Invoke(nameof(DelayedEnterBattle), 1f);
    }
    private void ReEnterBattleState()
    {
        if (stateMachine.currentState == battleState || stateMachine.currentState == attackState)
        {
            CancelInvoke(nameof(ReEnterBattleState));
            return;
        }

        stateMachine.ChangeState(battleState);
    }

}
