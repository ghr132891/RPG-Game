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

    /*
    private void Retreat()
    {
        lastTimeUsedRetreat = Time.time;
        stateMachine.ChangeState(enemyMage.mageRetreatState);

    }

    private bool CanUseRetreatAbility() => Time.time > lastTimeUsedRetreat + enemyMage.retreatCoolDown;
    */
}
