using UnityEngine;

public class Enemy_BattleState : EnemyState
{
    protected Transform player;
    protected Transform lastTarget;
    protected float lastTimeInBattle;
    protected float lastTimeAttacked = float.NegativeInfinity;
    
    public Enemy_BattleState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        UpdateBattleTimer();

        if (player == null)
            player = enemy.GetPlayerReference();

        if (ShouldRetreat())
        {
            ShortRetreat();
        }

        

    }
    protected void ShortRetreat()
    {
        float x = (enemy.reteratVelocity.x * enemy.activeSlowMultiplier) * -DirectionToPlayer();
        float y = enemy.reteratVelocity.y;

        rb.linearVelocity = new Vector2(x, y);
        enemy.HandleFlip(DirectionToPlayer());
    }

    public override void Update()
    {
        base.Update();


        if (enemy.PlayerDetected())
        {
            UpdateTargetIfNeeded();
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
            stateMachine.ChangeState(enemy.idleState);
       

        if (WithinAttackRange() && enemy.PlayerDetected() && CanAttack())
        {
            lastTimeAttacked = Time.time;
            ExecuteAttack();
        }
        else
        {
            float xVelocity = enemy.canChasePlayer ? enemy.GetBattleMoveSpeed() : 0.0001f;
            enemy.SetVelocity(xVelocity * DirectionToPlayer(), rb.linearVelocity.y);
        }

    }

    protected virtual void ExecuteAttack()
    {
        // 默认行为：进入普通近战攻击状态
        stateMachine.ChangeState(enemy.attackState);
    }
    protected bool CanAttack() => Time.time > lastTimeAttacked + enemy.attackCooldown;

    protected void UpdateTargetIfNeeded()
    {
        if(enemy.PlayerDetected() == false)
            return;

        Transform newTarget = enemy.PlayerDetected().transform;

        if(newTarget != lastTarget)
        {
            lastTarget = newTarget;
            player = newTarget;
        }



    }

    protected void UpdateBattleTimer() => lastTimeInBattle = Time.time;

    protected bool BattleTimeIsOver() => Time.time > lastTimeInBattle + enemy.battleTimeDuration;

    protected bool WithinAttackRange() => DistanceToPlayer() < enemy.attackDistance;

    protected bool ShouldRetreat() => DistanceToPlayer() < enemy.minAbleRetreatDistance;

    protected float DistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;

        return Mathf.Abs(player.position.x - enemy.transform.position.x);
    }

    protected int DirectionToPlayer()
    {
        if (player == null)
            return 0;

        float verticalDistance = Mathf.Abs(player.position.y - enemy.transform.position.y);
        float horizonalDistance = Mathf.Abs(player.position.x - enemy.transform.position.x);
        if (verticalDistance > .1f && horizonalDistance < 0.1f)
            return 0;

        int dir = player.position.x > enemy.transform.position.x ? 1 : -1;


        return dir;
    }

}
