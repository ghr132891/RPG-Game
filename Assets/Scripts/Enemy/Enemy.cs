using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : Entity
{
    public Entity_Stats stats { get; private set; }

    public Enemy_Health health { get; private set; }
    public Entity_Combat combat { get; private set; }
    public Entity_VFX vfx { get; private set; }
    public Enemy_IdleState idleState;
    public Enemy_MoveState moveState;
    public Enemy_AttackState attackState;
    public Enemy_BattleState battleState;
    public Enemy_DeadState deadState;
    public Enemy_StunnedState stunnedState;

    [Header("Battle Details")]
    public float battleMoveSpeed = 3;
    public float attackDistance = 2;
    public float attackCooldown = .5f;
    public bool canChasePlayer = true;
    [Space]
    public float battleTimeDuration = 5;
    public float minAbleRetreatDistance = 1;
    public Vector2 reteratVelocity;

    [Header("Stunned State Details")]
    public float stunnedDuration = 1;
    public Vector2 stunnedVelocity = new Vector2(7, 7);
    [SerializeField] protected bool canBeStunned;

    [Header("Movement Details")]
    public float idleTime = 2;
    public float moveSpeed = 1.3f;
    [Range(0, 2)]
    public float moveAnimSpeedMultplier = 1;

    [Header("Player Detection")]
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private Transform playerCheck;
    [SerializeField] private float playerCheckDistance = 10;

    [Header("Direction Setup")]
    [Tooltip("勾选此项，怪物一出生就会朝左，且不会搞乱内部的 facingDir 逻辑")]
    public bool faceLeftAtStart = true; // 默认设为 true，满足你所有的怪物初始朝左的需求
    public Transform player { get; private set; }

    public float activeSlowMultiplier { get; private set; } = 1;
    public float GetMoveSpeed() => moveSpeed * activeSlowMultiplier;
    public float GetBattleMoveSpeed() => battleMoveSpeed * activeSlowMultiplier;

    protected override void Awake()
    {
        base.Awake();
        health = GetComponent<Enemy_Health>();
        stats = GetComponent<Entity_Stats>();
        combat = GetComponent<Entity_Combat>();
        vfx = GetComponent<Entity_VFX>();

    }

    protected override void Start()
    {
        base.Start();

        // ========================================================
        // 【新增】：初始化怪物朝向
        // 如果要求朝左，且当前内部逻辑仍然是朝右(facingDir == 1)
        // ========================================================
        if (faceLeftAtStart && facingDir == 1)
        {
            Flip(); // 调用 Entity 父类的 Flip() 方法
                    // 这会同时把 facingDir 变成 -1，facingRight 变成 false，并自动旋转模型
        }
    }

    protected override void Update()
    {
        // 1. 必须保留 base.Update()，这样状态机才能正常运行
        base.Update();

        // 2. 每帧检测是否被卡进墙里
        CheckIfStuckInGround();
    }

    private void CheckIfStuckInGround()
    {
        if (stateMachine.currentState == deadState) return;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            // 1. 获取中心点极其微小范围内，所有属于 whatIsGround 图层的碰撞体
            Collider2D[] colliders = Physics2D.OverlapCircleAll(col.bounds.center, 0.02f, whatIsGround);

            bool isStuck = false;

            // 2. 遍历所有碰到的“地面/墙体”
            foreach (Collider2D hitCol in colliders)
            {
                // 【核心修复】：如果碰到的墙体，不是自己 (this.gameObject)
                // 并且也不是挂在自己身上的子物体 (比如史莱姆的 platformCollider)
                if (hitCol.gameObject != this.gameObject && !hitCol.transform.IsChildOf(this.transform))
                {
                    // 那说明是真的碰到了外面的墙壁！
                    isStuck = true;
                    break;
                }
            }

            // 3. 只有真正卡进外墙，才执行秒杀
            if (isStuck)
            {
                Debug.Log($"{gameObject.name} 被真的墙体卡住，执行防卡死秒杀！");
                EntityDeath();
            }
        }
    }

    public virtual void SpecialAttack()
    {

    }
    protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {

        activeSlowMultiplier = 1 - slowMultiplier;
        anim.speed = anim.speed * activeSlowMultiplier;

        yield return new WaitForSeconds(duration);

        StopSlowDown();
    }

    public override void StopSlowDown()
    {
        activeSlowMultiplier = 1;
        anim.speed = 1;
        base.StopSlowDown();

    }
    public void EnableCounterWindow(bool enable) => canBeStunned = enable;
    protected virtual void HandlePlayerDeath()
    {
        stateMachine.ChangeState(idleState);
    }
    public override void EntityDeath()
    {
        base.EntityDeath();
        stateMachine.ChangeState(deadState);
    }

    public void DestoryGameObjectWithDealy(float delay = 10)
    {
        Destroy(gameObject, delay);
    }

    public void TryEnterBattleState(Transform player)
    {
        if (stateMachine.currentState == battleState || stateMachine.currentState == attackState)
            return;

        this.player = player;
        stateMachine.ChangeState(battleState);
    }

    public Transform GetPlayerReference()
    {
        if (player == null)
            player = PlayerDetected().transform;

        return player;
    }





    public RaycastHit2D PlayerDetected()
    {
        // 1. 获取射线轨迹上的【所有】物体
        RaycastHit2D[] hits = Physics2D.RaycastAll(playerCheck.position, Vector2.right * facingDir, playerCheckDistance, whatIsPlayer | whatIsGround);

        foreach (RaycastHit2D hit in hits)
        {
            // 2. 忽略自己（主体碰撞体）和挂在身上的子物体（跳板 platformCollider）
            if (hit.collider.gameObject == this.gameObject || hit.collider.transform.IsChildOf(this.transform))
            {
                continue; // 透明穿透，继续往后看
            }

            // 3. 视线穿透自己后，碰到的第一个物体如果是玩家，说明看到了！
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                return hit;
            }

            // 4. 如果穿透自己后，碰到的第一个物体是真正的墙壁/地面，说明视线被墙挡住了
            if (((1 << hit.collider.gameObject.layer) & whatIsGround) != 0)
            {
                return default;
            }
        }

        // 什么都没看到
        return default;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // 【删除】这里也有 actualFacingDir 镜像判断，删掉，直接用 facingDir
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * playerCheckDistance), playerCheck.position.y));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * attackDistance), playerCheck.position.y));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * minAbleRetreatDistance), playerCheck.position.y));
    }

    // 默认情况下，所有怪物硬直结束后都进入 Idle 状态
    public virtual void OnStunFinished()
    {
        stateMachine.ChangeState(idleState);
    }
    private void OnEnable()
    {
        Player.OnPlayerDeadth += HandlePlayerDeath;
    }
    private void OnDisable()
    {
        Player.OnPlayerDeadth -= HandlePlayerDeath;
    }

   
}
