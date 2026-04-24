using UnityEngine;

public class Enemy_Archer : Enemy
{
    public bool CanBeCountered { get => canBeStunned; }
    public Enemy_ArcherBattleState archerBattleState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        idleState = new Enemy_IdleState(this, stateMachine, "idle");
        moveState = new Enemy_MoveState(this, stateMachine, "move");
        attackState = new Enemy_AttackState(this, stateMachine, "attack");
        deadState = new Enemy_DeadState(this, stateMachine, "idle");
        stunnedState = new Enemy_StunnedState(this, stateMachine, "stunned");

        archerBattleState = new Enemy_ArcherBattleState(this, stateMachine, "battle");
        battleState = archerBattleState;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idleState);
    }

    public void HandleCounter()
    {
        if (CanBeCountered == false)
            return;

        stateMachine.ChangeState(stunnedState);
    }
}
