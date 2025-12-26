using System.Collections;
using UnityEngine;

public class Enemy : Entity
{
    public Enemy_IdleState idleState;
    public Enemy_MoveState moveState;
    public Enemy_AttackState attackState;
    public Enemy_BattleState battleState;
    public Enemy_DeadState deadState;
    public Enemy_StunnedState stunnedState;

    [Header("Battle Details")]
    public float battleMoveSpeed = 3;
    public float attackDistance = 2;
    public float battleTimeDuration = 5;
    public float minAbleRetreatDistance = 1;
    public Vector2 reteratVelocity;

    [Header("Stunned State Details")]
    public float stunnedDuration = 1;
    public Vector2 stunnedVelocity = new Vector2(7,7);
    [SerializeField] protected bool canBeStunned;

    [Header("Movement Details")]
    public float idleTime = 2;
    public float moveSpeed = 1.3f;
    [Range(0,2)]
    public float moveAnimSpeedMultplier = 1;

    [Header("Player Detection")]
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private Transform playerCheck;
    [SerializeField] private float playerCheckDistance = 10;
    public Transform player { get; private set; }

    public float activeSlowMultiplier { get; private set; } = 1;
    public float GetMoveSpeed() => moveSpeed * activeSlowMultiplier;
    public float GetBattleMoveSpeed() => battleMoveSpeed * activeSlowMultiplier;


    protected override IEnumerator SlowDownEntityCo(float duration, float slowMultiplier)
    {
       
        activeSlowMultiplier = 1- slowMultiplier;     
        anim.speed =  anim.speed * activeSlowMultiplier;

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
    private void HandlePlayerDeath()
    {
        stateMachine.ChangeState(idleState);
    }
    public override void EntityDeath()
    {
        base.EntityDeath();
        stateMachine.ChangeState(deadState);
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
        if(player == null)
            player = PlayerDetected().transform;

        return player;
    }
    

    

    
    public RaycastHit2D PlayerDetected()
    {
        RaycastHit2D hit = 
            Physics2D.Raycast(playerCheck.position, Vector2.right * facingDir, playerCheckDistance, whatIsPlayer | whatIsGround);

        if (hit.collider == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
            return default;
        return hit;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * playerCheckDistance), playerCheck.position.y));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * attackDistance), playerCheck.position.y));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerCheck.position, new Vector3(playerCheck.position.x + (facingDir * minAbleRetreatDistance), playerCheck.position.y));
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
