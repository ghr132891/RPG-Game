using UnityEngine;

public class Enemy_SlimeDeadState : Enemy_DeadState
{
    private Enemy_Slime enemySlime;
    public Enemy_SlimeDeadState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemySlime = enemy as Enemy_Slime;
    }

    public override void Enter()
    {
        base.Enter();
        enemySlime.CreateSlimeOnDeath();
    }
}
