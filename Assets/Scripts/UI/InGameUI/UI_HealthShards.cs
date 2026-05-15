using UnityEngine;
using System.Collections.Generic;

public class UI_HealthShards : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject shardPrefab; // 现在的Prefab需要挂载 UI_HealthShard_Item 和 Animator

    private List<UI_HealthShard_Item> shards = new List<UI_HealthShard_Item>();
    private Entity_Health playerHealth;

    private System.Collections.IEnumerator Start()
    {
        yield return null; // 等待Player初始化

        if (Player.instance != null)
        {
            playerHealth = Player.instance.health;
            playerHealth.OnHealthUpdate += UpdateShards;
            InitializeShards();
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnHealthUpdate -= UpdateShards;
    }

    private void InitializeShards()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        shards.Clear();

        int maxHealth = Mathf.RoundToInt(Player.instance.stats.GetMaxHealth());
        int currentHealth = Mathf.Max(0, Mathf.RoundToInt(playerHealth.GetCurrentHealth()));

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newShardObj = Instantiate(shardPrefab, transform);
            UI_HealthShard_Item shardItem = newShardObj.GetComponent<UI_HealthShard_Item>();

            // 初始化时：索引小于当前血量的为满，否则为空
            shardItem.Initialize(i < currentHealth);
            shards.Add(shardItem);
        }
    }

    public void UpdateShards()
    {
        if (playerHealth == null) return;

        int currentHealth = Mathf.Max(0, Mathf.RoundToInt(playerHealth.GetCurrentHealth()));

        for (int i = 0; i < shards.Count; i++)
        {
            // 逐个通知碎片它当前应该处于什么状态，碎片内部会自己判断是否需要播放碎裂动画
            shards[i].UpdateState(i < currentHealth);
        }
    }
}