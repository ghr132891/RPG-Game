using UnityEngine;

public class Enemy_AttackState : EnemyState
{
    public Enemy_AttackState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        SyncAttackSpeed();
    }

    public override void Update()
    {
        base.Update();
        if (triggerCalled)
            stateMachine.ChangeState(enemy.battleState);
    }

}
