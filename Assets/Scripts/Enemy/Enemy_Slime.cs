using UnityEngine;

public class Enemy_Slime : Enemy, ICounterable
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_SlimeDeadState slimeDeadState { get; set; }
    private Transform targetToChase;

    [Header("Slime Specifics")]
    [SerializeField] private GameObject childSlimePrefab;
    [SerializeField] private int amountOfSlimeToCreate = 2;

    [SerializeField] private bool hasStunRecoveryAnimation = true;

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
