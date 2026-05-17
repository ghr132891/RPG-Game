using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpikeTrap : MonoBehaviour
{
    [Header("地刺伤害设置")]
    [SerializeField] private float physicalDamage = 20f;
    [SerializeField] private float elementalDamage = 0f;
    [SerializeField] private ElementType element = ElementType.None;

    [Header("触发机制")]
    [Tooltip("地刺对同一个目标的伤害间隔（冷却时间）")]
    [SerializeField] private float damageCooldown = 1f;

    // 记录正在地刺上的所有碰撞体
    private List<Collider2D> targetsInTrap = new List<Collider2D>();

    // 记录每一个目标的最后一次受伤时间（字典结构：目标 -> 最后受伤时间）
    // 这样可以实现多个敌人同时踩上去，各自独立计算冷却
    private Dictionary<IDamagable, float> targetLastDamageTimes = new Dictionary<IDamagable, float>();

    private void Update()
    {
        // 倒序遍历列表，防止在遍历过程中物体被销毁导致的报错
        for (int i = targetsInTrap.Count - 1; i >= 0; i--)
        {
            Collider2D col = targetsInTrap[i];

            // 如果物体已经不存在了（比如敌人死在地刺上被 Destroy 了），就移出列表
            if (col == null)
            {
                targetsInTrap.RemoveAt(i);
                continue;
            }

            TryDealDamage(col);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 【新增逻辑】：专属判定！如果是史莱姆踩上去了
        Enemy_Slime slime = collision.GetComponent<Enemy_Slime>();
        if (slime != null)
        {
            // 让史莱姆卡住，然后直接 return 结束方法！
            // 这样它就不会被加入 targetsInTrap 列表，完美免疫地刺伤害！
            slime.GetStuckOnSpike();
            return;
        }

        // 2. 原本的逻辑：对玩家和其他怪物造成伤害
        if (!targetsInTrap.Contains(collision))
        {
            targetsInTrap.Add(collision);
            TryDealDamage(collision);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 【新增逻辑】：如果史莱姆被击飞或者离开了尖刺，解除卡住状态
        Enemy_Slime slime = collision.GetComponent<Enemy_Slime>();
        if (slime != null)
        {
            slime.UnstuckFromSpike();
            return;
        }

        // 原本的逻辑
        if (targetsInTrap.Contains(collision))
        {
            targetsInTrap.Remove(collision);

            IDamagable damagableTarget = collision.GetComponent<IDamagable>();
            if (damagableTarget != null && targetLastDamageTimes.ContainsKey(damagableTarget))
            {
                targetLastDamageTimes.Remove(damagableTarget);
            }
        }
    }

    private void TryDealDamage(Collider2D collision)
    {
        IDamagable damagableTarget = collision.GetComponent<IDamagable>();

        if (damagableTarget != null)
        {
            // 尝试获取这个目标上一次的受伤时间，如果从来没受过伤，就默认为 0
            float lastDamageTime = targetLastDamageTimes.ContainsKey(damagableTarget) ? targetLastDamageTimes[damagableTarget] : 0f;

            // 检查这个目标的专属冷却时间是否结束
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                bool damageDealt = damagableTarget.TakeDamage(physicalDamage, elementalDamage, element, transform);

                // 只有当真正造成了伤害（比如没有被无敌帧挡住、没被闪避掉），才刷新时间
                if (damageDealt)
                {
                    targetLastDamageTimes[damagableTarget] = Time.time;
                    // 如果有陷阱触发的动画/音效，可以在这里播放
                }
            }
        }
    }
}