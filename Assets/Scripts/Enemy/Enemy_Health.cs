using UnityEngine;

public class Enemy_Health : Entity_Health
{

    private Enemy enemy ;

    [Header("Enemy UI Settings")]
    [SerializeField] private bool neverShowHealthBar = false; // 是否彻底隐藏怪物血条（勾选后怎么打都不显示）
    [SerializeField] private float showDurationAfterDamage = 3f; // 受击后血条的停留时间

    private float hideTimer;
    private bool isHealthBarVisible;

    protected override void Start()
    {
        base.Start();
        enemy = GetComponent<Enemy>();

        // 怪物生成时，默认关闭头顶的血条
        SetEnemyHealthBarVisible(false);

    }

    private void Update()
    {
        // 处理血条显示的倒计时逻辑
        if (isHealthBarVisible && hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                SetEnemyHealthBarVisible(false); // 倒计时结束，隐藏血条
            }
        }
    }
    public override bool TakeDamage(float damage, float elementalDamage,ElementType element, Transform damageDealer)
    {
        if(canTakeDamage == false)
            return false;

        bool wasHit = base.TakeDamage(damage, elementalDamage, element,damageDealer);

        if (wasHit == false)
            return false;

        // 如果没有被设置为“永久隐藏”，则在受击时激活血条并重置计时器
        if (!neverShowHealthBar)
        {
            SetEnemyHealthBarVisible(true);
            hideTimer = showDurationAfterDamage;
        }

        if (damageDealer.GetComponent<Player>() != null)
            enemy.TryEnterBattleState(damageDealer);
        return true;
      
    }

    /// <summary>
    /// 提供给外部或内部调用的方法：单独控制这个怪物的血条显示状态
    /// </summary>
    public void SetEnemyHealthBarVisible(bool isVisible)
    {
        isHealthBarVisible = isVisible;
        EnableHealthBar(isVisible); // 调用基类 Entity_Health 里的方法真正关闭 UI
    }

}
