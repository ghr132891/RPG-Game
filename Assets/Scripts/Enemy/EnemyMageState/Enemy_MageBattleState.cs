using UnityEngine;

public class Enemy_MageBattleState : Enemy_BattleState
{
    private Enemy_Mage enemyMage;
    //private float lastTimeUsedRetreat = float.NegativeInfinity;

    public Enemy_MageBattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as Enemy_Mage;
    }

    public override void Enter()
    {
        base.Enter();
        /*
        if (ShouldRetreat())
        {
            if (CanUseRetreatAbility())
                Retreat();
            else
                ShortRetreat();
        }
        */

    }
    public override void Update()
    {
        base.Update();

        
    }

    protected override void ExecuteAttack()
    {
        // 到了该攻击的时候，法师在这里掷骰子
        if (Random.value < enemyMage.dashAttackProbability)
        {
            // 抽中概率，进入新加的“后退冲刺斩击”状态
            stateMachine.ChangeState(enemyMage.mageDashAttackState);
        }
        else
        {
            // 没抽中，乖乖进入普通的普通攻击状态
            stateMachine.ChangeState(enemy.attackState);
        }
    }

    /*
    private void Retreat()
    {
        lastTimeUsedRetreat = Time.time;
        stateMachine.ChangeState(enemyMage.mageRetreatState);

    }

    private bool CanUseRetreatAbility() => Time.time > lastTimeUsedRetreat + enemyMage.retreatCoolDown;
    */
}
