using UnityEngine;

public class Enemy_MageRetreatState : EnemyState
{
    private Enemy_Mage enemyMage;
    private Vector3 startPosition;
    private Transform player;
    private int retreatDir; // 新增：用于缓存撤退方向
    public Enemy_MageRetreatState(Enemy enemy, StateMachine stateMachine, string animBoolName) : base(enemy, stateMachine, animBoolName)
    {
        enemyMage = enemy as Enemy_Mage;
    }

    public override void Enter()
    {
        base.Enter();

        if (player == null)
            player = enemy.GetPlayerReference();

        startPosition = enemy.transform.position;

        retreatDir = DirectionToPlayer();
        
        enemy.HandleFlip(DirectionToPlayer());

        enemy.gameObject.layer = LayerMask.NameToLayer("UnTargetable");
        enemy.vfx.DoImageEchoEffect(1f);
    }

    public override void Update()
    {
        base.Update();

        rb.linearVelocity = new Vector2(enemyMage.retreatSpeed * -retreatDir, rb.linearVelocity.y);
        bool rechedRetreatDistance = Vector2.Distance(enemy.transform.position, startPosition) >= enemyMage.retreatMaxDistance;

        if (rechedRetreatDistance || enemyMage.CanNotMoveBackwards())
            stateMachine.ChangeState(enemyMage.mageSpellCastState);

        


    }
    public override void Exit()
    {
        base.Exit();
        enemy.vfx.StopImageEchoEffect();
        enemy.gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    protected int DirectionToPlayer()
    {
        if (player == null)
            return enemy.facingDir;

        float verticalDistance = Mathf.Abs(player.position.y - enemy.transform.position.y);
        float horizonalDistance = Mathf.Abs(player.position.x - enemy.transform.position.x);
        if (verticalDistance > .1f && horizonalDistance < 0.1f)
            return enemy.facingDir;

        int dir = player.position.x > enemy.transform.position.x ? 1 : -1;


        return dir;
    }
}
