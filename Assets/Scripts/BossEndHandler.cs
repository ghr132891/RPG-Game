using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossEndHandler : MonoBehaviour
{
    [Header("核心引用")]
    [Tooltip("把场景中的 Mage 拖到这里")]
    [SerializeField] private Enemy_Mage bossMage;

    [Header("UI 设置")]
    [Tooltip("游戏结束/通关的 UI 面板或按钮物体（默认隐藏）")]
    [SerializeField] private GameObject gameCompletePanel;
    [Tooltip("面板上的结束按钮")]
    [SerializeField] private Button endButton;

    private void Start()
    {
        if (bossMage != null)
        {
            // 监听组件自带的死亡事件
            Entity_Health bossHealth = bossMage.GetComponent<Entity_Health>();
            if (bossHealth != null)
            {
                bossHealth.OnDie += HandleBossDeath;
            }
        }

        if (endButton != null)
        {
            // 绑定按钮点击事件
            endButton.onClick.AddListener(OnEndButtonClick);
        }

        if (gameCompletePanel != null)
        {
            gameCompletePanel.SetActive(false); // 确保一开始是隐藏的
        }
    }

    private void HandleBossDeath()
    {
        // 启动倒计时协程
        StartCoroutine(ShowEndUiWithDelay(4f));
    }

    private IEnumerator ShowEndUiWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (gameCompletePanel != null)
        {
            gameCompletePanel.SetActive(true);

<<<<<<< Updated upstream
            // ================= 【核心新增：禁用玩家输入】 =================
            // 找到场景中的玩家实体
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                // 1. 强制清空玩家的物理速度，防止出现“溜冰”滑行
                player.rb.linearVelocity = Vector2.zero;

                // 2. 强制切换回站立状态（防止玩家刚好在空中或攻击时卡住动画）
                player.stateMachine.ChangeState(player.idleState);

                // 3. 直接禁用 Player 脚本，彻底切断所有 Update 里的输入检测！
                player.enabled = false;
            }
            // ==============================================================

            // 如果你希望弹出按钮时，除了玩家不能动，连游戏世界的时间（背景、怪物等）也彻底定格，
            // 可以解除下面这行代码的注释：
            // Time.timeScale = 0f;
        }

=======
            // 如果你希望弹出按钮时游戏暂停，可以解锁下面这行：
            // Time.timeScale = 0f; 
        }
>>>>>>> Stashed changes
    }

    private void OnEndButtonClick()
    {
        // 如果之前暂停了游戏，先恢复时间流速
        Time.timeScale = 1f;

        Debug.Log("游戏通关！正在清空存档并返回主菜单...");

        // 1. 调用数据管理系统清空存档
        if (SaveManager.instance != null)
        {
            // 提示：根据你 SaveManager 的具体方法名调整，通常是 DeleteSaveData() 或 NewGame()
            SaveManager.instance.DeleteSaveData();
        }

        // 2. 调用 GameManager 返回主菜单
        if (GameManager.instance != null)
        {
            GameManager.instance.ChangeScene("MainMenu", RespawnType.NoneSpecific);
        }
    }

    private void OnDestroy()
    {
        // 销毁时移除监听，防止内存泄漏
        if (bossMage != null)
        {
            Entity_Health bossHealth = bossMage.GetComponent<Entity_Health>();
            if (bossHealth != null)
            {
                bossHealth.OnDie -= HandleBossDeath;
            }
        }
    }
}