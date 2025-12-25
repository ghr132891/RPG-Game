using UnityEngine;

public class SkillObject_TimeEcho : SkillObject_Base
{
    [SerializeField] private float wispMoveSpeed = 15f;
    [SerializeField] private GameObject onDeathVfx;
    [SerializeField] private LayerMask whatIsGround;
    private Skill_TimeEcho echoManager;
    private TrailRenderer wispTrail;
    private Transform playerTransform;
    private Entity_Health playerHealth;
    private Player_SkillManager skillManager;
    private Entity_StatusHandler statusHandler;
    private SkillObject_Health echoHealth;

    private bool shouldeToMovePlayer;

    public int maxAttacks { get; private set; }
    public void SetupEcho(Skill_TimeEcho echoManager)
    {
        this.echoManager = echoManager;
        playerHealth = echoManager.player.health;
        skillManager = echoManager.skillManager;
        statusHandler = echoManager.player.statusHandler;
        playerTransform = echoManager.transform.root;
        maxAttacks = echoManager.GetMaxAttacks();
        playerStats = echoManager.player.stats;
        damageScaleData = echoManager.damageScaleData;
        Invoke(nameof(HandleDeath), echoManager.GetEchoDuration());
        FlipToTarget();

        echoHealth = GetComponent<SkillObject_Health>();
        wispTrail = GetComponentInChildren<TrailRenderer>();
        wispTrail.gameObject.SetActive(false);

        anim.SetBool("canAttack", maxAttacks > 0);

    }

    private void Update()
    {
        if (shouldeToMovePlayer)
        {
            HandleWispMovement();
        }
        else
        {
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
            StopHorizontalMovement();
        }
    }

    private void HandlePlayerTouch()
    {
        float healAmount = echoHealth.lastDamageTaken * echoManager.GetPercentOfDamageHealed();
        playerHealth.IncreaseHealth(healAmount);

        float amountInSeconds = echoManager.GetCooldownReduceInSeconds();
        echoManager.ReduceCooldownBy(amountInSeconds);

        if(echoManager.CanRemoveNegativeEffects())
            statusHandler.RemoveAllNegativeEffects();

    }

    private void HandleWispMovement()
    {
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, wispMoveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, playerTransform.position) < .5f)
        {
            HandlePlayerTouch();
            Destroy(gameObject);
        }       
    }
    
    private void FlipToTarget()
    {
        Transform target = FindClosestTarget();
        if (target != null && target.position.x < transform.position.x)
            transform.Rotate(0, 180, 0);
    }

    public void PerformAttacks()
    {
        DamageEnemiesInRadius(targetCheck, 1);

        if (targetGotHit == false)
            return;

        bool canDuplicate = Random.value < echoManager.GetDuplicateChance();
        float xOffect = transform.position.x < lastTarget.position.x ? 1 : -1;
        if (canDuplicate)
            echoManager.CreatTimeEcho(lastTarget.position + new Vector3(xOffect, 0));

    }
    public void HandleDeath()
    {
        Instantiate(onDeathVfx, transform.position, Quaternion.identity);

        if (echoManager.shouldBeWisp())
        {
            TurnInToWisp();
        }
        else
            Destroy(gameObject);
    }

    private void TurnInToWisp()
    {
        shouldeToMovePlayer = true;
        anim.gameObject.SetActive(false);
        wispTrail.gameObject.SetActive(true);
        rb.simulated = false;
    }

    private void StopHorizontalMovement()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f);

        if (hit.collider != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

    }

}
