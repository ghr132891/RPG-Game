using UnityEngine;

public class Enemy_Archer : Enemy
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_ArcherBattleState archerBattleState { get; private set; }


    [Header("Archer Specifics")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform arrowStartPoint;
    [SerializeField] private float arrowSpeed = 8f;

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        deadState = new Enemy_DeadState(this, stateMachine, "dead");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

        archerBattleState = new Enemy_ArcherBattleState(this, stateMachine, "battle");
        battleState = archerBattleState;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public override void SpecialAttack()
    {
        GameObject newArrow = Instantiate(arrowPrefab, arrowStartPoint.position, Quaternion.identity);
        newArrow.GetComponent<Enemy_ArcherArrow>().SetUpArrow(arrowSpeed * facingDir,combat);
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;
        // 【新增】：触发瞬间白光
        if (vfx is Enemy_VFX enemyVfx)
        {
            enemyVfx.PlayParryFlash();
        }

        stateMachine.ChangeState(stunnedState);
    }
}
