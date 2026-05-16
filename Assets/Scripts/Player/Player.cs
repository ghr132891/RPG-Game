using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Entity
{
    public static Player instance;
    public static event Action OnPlayerDeadth;
    public  UI ui {  get; private set; }
    public Player_SkillManager skillManager;
    public Player_VFX vfx { get; private set; }
    public Entity_Health health { get; private set; }
    public Entity_StatusHandler statusHandler { get; private set; }
    public Player_Combat combat { get; private set; }
    public Inventory_Player inventory { get; private set; }
    public Player_Stats stats { get; private set; }

    #region State Variables
    public PlayerInputSet input { get; private set; }
    public Player_IdleState idleState { get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_JumpState jumpState { get; private set; }
    public Player_FallState fallState { get; private set; }
    public Player_WallSlideState wallSlideState { get; private set; }
    public Player_WallJumpState wallJumpState { get; private set; }
    public Player_DashState dashState { get; private set; }
    public Player_BasicAttackState basicAttackState { get; private set; }
    public Player_JumpAttackState jumpAttackState { get; private set; }
    public Player_DeadState deadState { get; private set; }
    public Player_CounterAttackState counterAttackState { get; private set; }

    public Player_SwordThrowState swordThrowState { get; private set; }
    public Player_DomainExpansionState domainExpansionState { get; private set; }
    #endregion


    [Header("Attack Details")]
    public Vector2[] attackVelocity;
    public Vector2 jumpAttackVelocity;
    public float attackVelocityDuration = .1f;
    public float comboResetTime = 1;
    private Coroutine queuedAttackCo;

    [Header("Ultimate ability details")]
    public float riseSpeed = 25f;
    public float riseMaxDistance = 3;

    [Header("Movement Details")]
    private float originalGravity;
    public float moveSpeed;
    public float jumpForce = 5;
    public Vector2 wallJumpForce;
    [Range(0, 1)]
    public float inAirMoveMultiplier = .7f;
    [Range(0, 1)]
    public float wallSlideSlowMultiplier = .7f;
    [Space]
    public float dashDuration = .25f;
    public float dashSpeed = 20;

    public float activeSlowMultiplier { get; private set; } =1;
    public float TimeScaleFix => (WorldManager.Instance != null && Time.timeScale < 1f)
        ? (1f / WorldManager.Instance.timeWorldScale) : 1f;
    public float GetMoveSpeed() => moveSpeed * activeSlowMultiplier * TimeScaleFix;
    public float GetJumpForce() => jumpForce * activeSlowMultiplier * TimeScaleFix;
    public float GetDashSpeed() => dashSpeed * activeSlowMultiplier * TimeScaleFix;
    public Vector2 GetWallJumpForce() => wallJumpForce * activeSlowMultiplier * TimeScaleFix;
    public Vector2 GetJumpAttackVelocity() => jumpAttackVelocity * activeSlowMultiplier * TimeScaleFix;

    [Header("Input Info")]
    public Vector2 moveInput { get; private set; }
    private Vector2 rawMoveInput;
    public float xInput{ get; private set; }
    public float yInput{ get; private set; }

    public Vector2 mousePosition { get; private set; }

    // ===================================
    // ======= 动作游戏核心：卡肉感 =======
    // ===================================
    private Coroutine hitStopCo;
    // 【新增】：用于告诉外界（比如风场），我正在放重砸技能！
    public bool isPlunging;

    protected override void Awake()
    {
        base.Awake();
        instance = this;


        ui = FindAnyObjectByType<UI>();
        skillManager = GetComponent<Player_SkillManager>();
        vfx = GetComponent<Player_VFX>();
        health = GetComponent<Entity_Health>();
        statusHandler = GetComponent<Entity_StatusHandler>();
        combat = GetComponent<Player_Combat>();
        inventory = GetComponent<Inventory_Player>();
        stats = GetComponent<Player_Stats>();

        input = new PlayerInputSet();
        ui.SetupControlUI(input);

        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
        jumpState = new Player_JumpState(this, stateMachine, "jumpFall");
        fallState = new Player_FallState(this, stateMachine, "jumpFall");
        wallSlideState = new Player_WallSlideState(this, stateMachine, "wallSlide");
        wallJumpState = new Player_WallJumpState(this, stateMachine, "jumpFall");
        dashState = new Player_DashState(this, stateMachine, "dash");
        basicAttackState = new Player_BasicAttackState(this, stateMachine, "basicAttack");
        jumpAttackState = new Player_JumpAttackState(this, stateMachine, "jumpAttack");
        deadState = new Player_DeadState(this, stateMachine, "dead");
        counterAttackState = new Player_CounterAttackState(this, stateMachine, "counterAttack");
        swordThrowState = new Player_SwordThrowState(this,stateMachine,"swordThrow");
        domainExpansionState = new Player_DomainExpansionState(this,stateMachine,"jumpFall");
    }

    protected override void Start()
    {
        base.Start();
        originalGravity = GetComponent<Rigidbody2D>().gravityScale;
        stateMachine.Initialize(idleState);
    }

    protected override void Update()
    {
        base.Update();
        ApplyWorldMovementLogic();
    }
    public void TeleportPlayer(Vector3 position) => transform.position = position;

    // 【新增】：获取受时间流速修正后的冲刺持续时间
    public float GetDashDuration()
    {
        return dashDuration * (WorldManager.Instance.currentWorld == WorldType.Time ? WorldManager.Instance.timeWorldScale : 1f);
    }
    private void ApplyWorldMovementLogic()
    {

        moveInput = rawMoveInput;
        // ==========================================
        // 【核心修复】：把输入的向量值，拆解同步给 xInput 和 yInput！
        // 只有这样，你的 Player_MoveState 和物理速度更新方法才能抓到真实的键盘输入
        // ==========================================
        xInput = moveInput.x;
        yInput = moveInput.y;


    }

    public void SetTimeImmunity(bool isImmune)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (isImmune)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;

            // 【终极解法】：重力必须按流速的平方倍增加，完美抵消慢动作下落！
            float ts = WorldManager.Instance.timeWorldScale;
            rb.gravityScale = originalGravity * (1f / (ts * ts));
        }
        else
        {
            anim.updateMode = AnimatorUpdateMode.Normal;
            rb.gravityScale = originalGravity; // 恢复重力
        }
    }

    protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
       
        Vector2[] originalAttackVelocity = attackVelocity;

        activeSlowMultiplier = 1 - slowMultiplier;

        anim.speed = anim.speed * activeSlowMultiplier;


        for (int i = 0; i < attackVelocity.Length; i++)
        {
            attackVelocity[i] = attackVelocity[i] * activeSlowMultiplier;
        }

        yield return new WaitForSeconds(duration);

        StopSlowDown();

        

        for (int i = 0; i < attackVelocity.Length; i++)
        {
            attackVelocity[i] = originalAttackVelocity[i];

        }
    }

    public override void StopSlowDown()
    {
        activeSlowMultiplier = 1;
        anim.speed = 1;
        base.StopSlowDown();
    }

    public override void EntityDeath()
    {
        base.EntityDeath();
        OnPlayerDeadth?.Invoke();
        stateMachine.ChangeState(deadState);
    }
    public void EnterAttackStateWithDelay()
    {
        if (queuedAttackCo != null)
            StopCoroutine(queuedAttackCo);

        queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayCo());
    }

    private IEnumerator EnterAttackStateWithDelayCo()
    {
        yield return new WaitForEndOfFrame();
        stateMachine.ChangeState(basicAttackState);
    }

    private void TryInteract()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;
        Collider2D[] objectsAround = Physics2D.OverlapCircleAll(transform.position,1f);

        foreach(var target in objectsAround)
        {
            IInteractable interactable = target.GetComponent<IInteractable>();
            if (interactable == null) continue;

            float distance = Vector2.Distance(transform.position, target.transform.position);

            if(distance < closestDistance)
            {
                closestDistance = distance;
                closest = target.transform;
            }
        }

        if (closest == null)
            return;
        closest.GetComponent<IInteractable>().Interact();
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Mouse.performed += ctx => mousePosition = ctx.ReadValue<Vector2>();

        input.Player.Movement.performed += ctx => rawMoveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => rawMoveInput = Vector2.zero;

        input.Player.Spell.performed += ctx => skillManager.shard.TryUseSkill();
        input.Player.Spell.performed += ctx => skillManager.timeEcho.TryUseSkill();

        input.Player.Interact.performed += ctx => TryInteract();

        //input.Player.QuickItemSlot_1.performed += ctx => inventory.TryUseQuickItemSlot(1);
        //input.Player.QuickItemSlot_2.performed += ctx => inventory.TryUseQuickItemSlot(2);

        input.Player.SwitchNormal.performed += ctx => skillManager.normalSwitch.TryUseSkill();
        input.Player.SwitchMirror.performed += ctx => skillManager.mirrorSwitch.TryUseSkill();
        input.Player.SwitchTime.performed += ctx => skillManager.timeSwitch.TryUseSkill();




    }

    private void OnDisable()
    {
        input.Disable();
    }

    public void TriggerHitStop(float duration)
    {
        // 如果当前已经在卡肉了，重新计时（防止连续弹反导致时间错乱）
        if (hitStopCo != null)
            StopCoroutine(hitStopCo);

        hitStopCo = StartCoroutine(HitStopRoutine(duration));
    }

    private System.Collections.IEnumerator HitStopRoutine(float duration)
    {
        // 1. 瞬间将时间极大幅度减慢（不要设为绝对的 0，设为 0.05f 可以让粒子特效有一点点动感）
        Time.timeScale = 0.05f;

        // 2. 极其关键：必须使用 WaitForSecondsRealtime 等待真实的物理时间！
        // 因为当前的 Time.timeScale 已经快变成 0 了，用普通的 WaitForSeconds 会永远等不到结束。
        yield return new WaitForSecondsRealtime(duration);

        // 3. 恢复时间。这里完美兼容你的世界切换系统：
        // 如果当前在时间世界，就恢复到减速状态；否则恢复到 1.0 正常速度。
        if (WorldManager.Instance != null && WorldManager.Instance.currentWorld == WorldType.Time)
        {
            Time.timeScale = WorldManager.Instance.timeWorldScale;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }


}
